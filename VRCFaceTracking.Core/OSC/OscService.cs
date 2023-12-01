using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core.OSC;

public class OscService : IParameterOutputService
{
    private static Socket _senderClient, _receiverClient;
    private static CancellationTokenSource _recvThreadCts;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly ILogger _logger;
    private readonly OscQueryConfigParser _oscQueryConfigParser;
    private readonly IParamSupervisor _paramSupervisor;
    private readonly QueryRegistrar _queryRegistrar;
    private HttpHandler _httpHandler;
    private readonly byte[] _recvBuffer = new byte[4096], _sendBuffer = new byte[4096];
    
    public OscService(
        ILocalSettingsService localSettingsService, 
        ILoggerFactory loggerFactory,
        OscQueryConfigParser oscQueryConfigParser,
        IParamSupervisor paramSupervisor
    )
    {
        _localSettingsService = localSettingsService;
        _oscQueryConfigParser = oscQueryConfigParser;
        _logger = loggerFactory.CreateLogger("OSC");
        OnMessageDispatched = () => { };
        OnAvatarLoaded = (_, _) => { };
        OnMessageReceived = HandleNewMessage;
        _paramSupervisor = paramSupervisor;
        _queryRegistrar = new QueryRegistrar();
    }

    private int _inPort;
    public int InPort
    {
        get => _inPort;
        set => SetField(ref _inPort, value);
    }

    private int _outPort;
    public int OutPort
    {
        get => _outPort;
        set => SetField(ref _outPort, value);
    }

    private string _destinationAddress;
    public string DestinationAddress
    {
        get => _destinationAddress;
        set => SetField(ref _destinationAddress, value);
    }

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => SetField(ref _isConnected, value);
    }

    public Action OnMessageDispatched
    {
        get;
        set;
    }

    public Action<OscMessage> OnMessageReceived
    {
        get;
        set;
    }

    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded
    {
        get;
        set;
    }

    public async Task SaveSettings()
    {
        await _localSettingsService.SaveSettingAsync("OSCAddress", DestinationAddress);
        await _localSettingsService.SaveSettingAsync("OSCInPort", InPort);
        await _localSettingsService.SaveSettingAsync("OSCOutPort", OutPort);

        await Task.CompletedTask;
    }
    
    public async Task LoadSettings()
    {
        DestinationAddress = await _localSettingsService.ReadSettingAsync("OSCAddress", "127.0.0.1");
        InPort = await _localSettingsService.ReadSettingAsync("OSCInPort", 9001);
        OutPort = await _localSettingsService.ReadSettingAsync("OSCOutPort", 9000);

        await Task.CompletedTask;
    }

    public async Task<(bool, bool)> InitializeAsync()
    {
        _logger.LogDebug("OSC Service Initializing");
            
        await LoadSettings();
        (bool listenerSuccess, bool senderSuccess) result = (false, false);

        if (string.IsNullOrWhiteSpace(DestinationAddress))
        {
            return result;  // Return both false as we cant bind to anything without an address
        }

        InitOscQuery();
            
        result = (BindListener(), BindSender());

        var isParsingAvatar = false;
        async void FirstClientDiscovered()
        {
            QueryRegistrar.OnVRCClientDiscovered -= FirstClientDiscovered;
                
            if (!isParsingAvatar && string.IsNullOrEmpty(OscQueryConfigParser.AvatarId))
            {
                isParsingAvatar = true;
                var newAvatar = await _oscQueryConfigParser.ParseNewAvatar(QueryRegistrar.VrchatClientEndpoint);
                if (newAvatar.HasValue)
                {
                    OnAvatarLoaded(newAvatar.Value.avatarInfo, newAvatar.Value.relevantParameters);
                }
            }
        }

        QueryRegistrar.OnVRCClientDiscovered += FirstClientDiscovered;
            
        _logger.LogDebug("OSC Service Initialized with result {0}", result);
        await Task.CompletedTask;
        return result;
    }
    
    private void InitOscQuery()
    {
        // Advertise our OSC JSON and OSC endpoints (OSC JSON to display the silly lil popup in-game)
        _queryRegistrar.Advertise("VRCFT", "_oscjson._tcp", 6970, IPAddress.Loopback);
        _queryRegistrar.Advertise("VRCFT", "_osc._udp", 6969, IPAddress.Loopback);

        _httpHandler?.Dispose();
        _httpHandler = new HttpHandler(6970);
    }

    private bool BindSender()
    {
        _logger.LogDebug("Binding Sender Client");
        _senderClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _senderClient.Connect(new IPEndPoint(IPAddress.Parse(DestinationAddress), OutPort));
        }
        catch(Exception e)
        {
            _logger.LogDebug("Failed to bind to port {0} with reason {1}", OutPort, e.Message);
            return false;
        }

        return true;
    }

    private bool BindListener()
    {
        _logger.LogTrace("Binding Receiver Client");
        _receiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _receiverClient.Bind(new IPEndPoint(IPAddress.Parse(DestinationAddress), InPort));

            StartListenerThread();
        }
        catch
        {
            _logger.LogError("Failed to bind to port {0}", InPort);
            // Now we find the app that's bound to the port and log it
            var p = new Process();
            p.StartInfo.FileName = "netstat.exe";
            p.StartInfo.Arguments = "-a -n -o";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
                
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
                
            // Find the line with the port we're trying to bind to
            var lines = output.Split('\n');
            foreach (var line in lines.Where(line => line.Contains(InPort.ToString())))
            {
                // Get the PID
                var pid = line.Split(' ').Last().Trim();
                // Get the process
                var proc = Process.GetProcessById(int.Parse(pid));
                _logger.LogError("Port {inPort} is already bound by {processName}", InPort, proc.ProcessName);
                break;
            }

            IsConnected = false;
            return false;
        }

        IsConnected = true;
        return true;
    }

    private void StartListenerThread()
    {
        _recvThreadCts?.Cancel();    // In theory, the closure of the client should have already cancelled the token, but just in case

        var newToken = new CancellationTokenSource();
        ThreadPool.QueueUserWorkItem(ct =>
        {
            var token = (CancellationToken)ct!;
            while (!token.IsCancellationRequested)
            {
                Recv();
            }
        }, newToken.Token);
        _recvThreadCts = newToken;
    }

        
    private void Recv()
    {
        try
        {
            var bytesReceived = _receiverClient.Receive(_recvBuffer, _recvBuffer.Length, SocketFlags.None);
            var offset = 0;
            var newMsg = OscMessage.TryParseOsc(_recvBuffer, bytesReceived, ref offset);
            if (newMsg == null)
            {
                return;
            }

            Array.Clear(_recvBuffer, 0, bytesReceived);
            OnMessageReceived(newMsg);
        }
        catch (Exception e)
        {
            // Ignore as this is most likely a timeout exception
            _logger.LogTrace("Failed to receive message: {0}", e.Message);
        }
    }
        
    private async void HandleNewMessage(OscMessage msg)
    {
        switch (msg.Address)
        {
            case "/avatar/change":
                if (msg._meta.ValueLength > 0 && msg.Value is string avatarId)
                {
                    var newAvatar  = await _oscQueryConfigParser.ParseNewAvatar(QueryRegistrar.VrchatClientEndpoint);
                    if (newAvatar.HasValue)
                    {
                        OnAvatarLoaded(newAvatar.Value.avatarInfo, newAvatar.Value.relevantParameters);
                    }
                }

                break;
            case "/vrcft/settings/forceRelevant":   // Endpoint for external tools to force vrcft to send all parameters
                if (msg._meta.ValueLength > 0 && msg.Value is bool relevancy)
                {
                    _paramSupervisor.AllParametersRelevant = relevancy;
                }

                break;
            /*
            case "/avatar/parameters/EyeTrackingActive":
                if (UnifiedLibManager.EyeStatus != ModuleState.Uninitialized)
                {
                    if (!msg.Value.BoolValue)
                        UnifiedLibManager.EyeStatus = ModuleState.Idle;
                    else UnifiedLibManager.EyeStatus = ModuleState.Active;
                }
                break;
            case "/avatar/parameters/LipTrackingActive":
            case "/avatar/parameters/ExpressionTrackingActive":
                if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized)
                {
                    if (!msg.Value.BoolValue)
                        UnifiedLibManager.ExpressionStatus = ModuleState.Idle;
                    else UnifiedLibManager.ExpressionStatus = ModuleState.Active;
                }
                break;
            */
        }
    }

    public void Send(OscMessage message)
    {
        var nextByteIndex = SROSCLib.create_osc_message(_sendBuffer, ref message._meta);
        if (nextByteIndex > 4096)
        {
            _logger.LogError("OSC message too large to send! Skipping this batch of messages.");
            return;
        }
        
        _senderClient.Send(_sendBuffer, nextByteIndex, SocketFlags.None);
        OnMessageDispatched();
    }
        
    public void Teardown()
    {
        // We just need to cancel our listener thread and close the sockets
        _logger.LogDebug("OSC Service Teardown");
        _recvThreadCts?.Cancel();
            
        _senderClient?.Close();
        _receiverClient?.Close();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}