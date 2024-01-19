using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core.OSC;

public partial class OscQueryService : ObservableObject
{
    // Services
    private readonly ILocalSettingsService _localSettingsService;
    private readonly ILogger _logger;
    private readonly OscQueryConfigParser _oscQueryConfigParser;
    private readonly ParamSupervisor _paramSupervisor;
    private readonly QueryRegistrar _queryRegistrar;
    
    // Delegates
    public Action OnMessageDispatched;
    public Action<OscMessage> OnMessageReceived;
    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded;
    
    [ObservableProperty] private bool _isConnected;
    
    [ObservableProperty]
    [property: SavedSetting("OSCInPort", 9001)] 
    private int _inPort;

    [ObservableProperty]
    [property: SavedSetting("OSCOutPort", 9000)]
    private int _outPort;

    [ObservableProperty]
    [property: SavedSetting("OSCAddress", "127.0.0.1")]
    private string _destinationAddress;
    
    // Recv and send buffers
    private readonly byte[] _recvBuffer = new byte[4096], _sendBuffer = new byte[4096];
 
    // Local vars
    private Socket _senderClient, _receiverClient;
    private CancellationTokenSource _recvThreadCts;
    private HttpHandler _httpHandler;
    
    public OscQueryService(
        ILocalSettingsService localSettingsService, 
        ILoggerFactory loggerFactory,
        OscQueryConfigParser oscQueryConfigParser,
        ParamSupervisor paramSupervisor
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

    public async Task SaveSettings()
    {
        await _localSettingsService.Save(this);

        await Task.CompletedTask;
    }
    
    public async Task LoadSettings()
    {
        await _localSettingsService.Load(this);

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

        QueryRegistrar.OnVrcClientDiscovered += FirstClientDiscovered;
            
        _logger.LogDebug("OSC Service Initialized with result {0}", result);
        await Task.CompletedTask;
        return result;

        void FirstClientDiscovered()
        {
            QueryRegistrar.OnVrcClientDiscovered -= FirstClientDiscovered;
                
            HandleNewAvatar();
        }
    }
    
    private void InitOscQuery()
    {
        // Advertise our OSC JSON and OSC endpoints (OSC JSON to display the silly lil popup in-game)
        QueryRegistrar.Advertise("_oscjson._tcp", "VRCFT", 6970, IPAddress.Loopback);
        QueryRegistrar.Advertise("_osc._udp", "VRCFT", 6969, IPAddress.Loopback);

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

    private async void HandleNewAvatar()
    {
        var newAvatar = await _oscQueryConfigParser.ParseNewAvatar(_queryRegistrar.VrchatClientEndpoint);
        if (newAvatar.HasValue)
        {
            OnAvatarLoaded(newAvatar.Value.avatarInfo, newAvatar.Value.relevantParameters);
        }
    }
        
    private void HandleNewMessage(OscMessage msg)
    {
        switch (msg.Address)
        {
            case "/avatar/change":
                HandleNewAvatar();
                break;
            case "/vrcft/settings/forceRelevant":   // Endpoint for external tools to force vrcft to send all parameters
                if (msg.Value is bool relevancy)
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
        var nextByteIndex = message.Encode(_sendBuffer);
        if (nextByteIndex > 4096)
        {
            _logger.LogError("OSC message too large to send! Skipping this batch of messages.");
            return;
        }
        
        _senderClient?.Send(_sendBuffer, nextByteIndex, SocketFlags.None);
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
}