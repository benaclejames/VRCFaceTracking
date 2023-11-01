using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.Query.mDNS;

namespace VRCFaceTracking.Core.OSC
{
    public class OscMain : IOSCService
    {
        private Socket _senderClient, _receiverClient;
        private static CancellationTokenSource _recvThreadCts;
        private readonly ILocalSettingsService _localSettingsService;
        private readonly ILogger _logger;
        private readonly ConfigParser _configParser;
        private readonly IParamSupervisor _paramSupervisor;
        private readonly QueryRegistrar _queryRegistrar;
        private HttpHandler _httpHandler;
        private readonly byte[] _oscBuffer = new byte[4096];
        
        public Action OnMessageDispatched { get; set; }
        public Action<OscMessage> OnMessageReceived { get; set; }
        public Action<bool> OnConnectedDisconnected { get; set; }
        public bool IsConnected { get; set; }


        public OscMain(ILocalSettingsService localSettingsService, ILoggerFactory loggerFactory,
            ConfigParser configParser, IParamSupervisor paramSupervisor)
        {
            _localSettingsService = localSettingsService;
            _configParser = configParser;
            _logger = loggerFactory.CreateLogger("OSC");
            OnMessageDispatched = () => { };
            OnMessageReceived = HandleNewMessage;
            OnConnectedDisconnected = b => {IsConnected = b;};
            _paramSupervisor = paramSupervisor;
            _queryRegistrar = new QueryRegistrar();
        }

        public int InPort { get; set; }
        public int OutPort { get; set; }
        public string Address { get; set; }


        public async Task LoadSettings()
        {
            var address = await _localSettingsService.ReadSettingAsync<string>("OSCAddress");
            Address = address ?? "127.0.0.1";

            var inPort = await _localSettingsService.ReadSettingAsync<int>("OSCInPort");
            InPort = inPort == default ? 9001 : inPort;

            var outPort = await _localSettingsService.ReadSettingAsync<int>("OSCOutPort");
            OutPort = outPort == default ? 9000 : outPort;

            await Task.CompletedTask;
        }

        public async Task SaveSettings()
        {
            await _localSettingsService.SaveSettingAsync("OSCAddress", Address);
            await _localSettingsService.SaveSettingAsync("OSCInPort", InPort);
            await _localSettingsService.SaveSettingAsync("OSCOutPort", OutPort);

            await Task.CompletedTask;
        }

        public async Task<(bool, bool)> InitializeAsync()
        {
            _logger.LogDebug("OSC Service Initializing");
            
            await LoadSettings();
            (bool listenerSuccess, bool senderSuccess) result = (false, false);

            if (string.IsNullOrWhiteSpace(Address))
            {
                return result;  // Return both false as we cant bind to anything without an address
            }

            InitOscQuery();
            
            result = (BindListener(), BindSender());

            var isParsingAvatar = false;
            void FirstClientDiscovered()
            {
                QueryRegistrar.OnVRCClientDiscovered -= FirstClientDiscovered;
                
                if (!isParsingAvatar && string.IsNullOrEmpty(ConfigParser.AvatarId))
                {
                    isParsingAvatar = true;
                    _configParser.ParseCurrentAvatar(QueryRegistrar.VrchatClientEndpoint);
                }
            }

            QueryRegistrar.OnVRCClientDiscovered += FirstClientDiscovered;
            
            _logger.LogDebug("OSC Service Initialized with result {0}", result);
            await Task.CompletedTask;
            return result;
        }

        public void Teardown()
        {
            // We just need to cancel our listener thread and close the sockets
            _logger.LogDebug("OSC Service Teardown");
            _recvThreadCts?.Cancel();
            
            _senderClient?.Close();
            _receiverClient?.Close();
        }

        private bool BindSender()
        {
            _logger.LogDebug("Binding Sender Client");
            _senderClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                _senderClient.Connect(new IPEndPoint(IPAddress.Parse(Address), OutPort));
            }
            catch(Exception e)
            {
                _logger.LogDebug("Failed to bind to port {0} with reason {1}", OutPort, e.Message);
                return false;
            }

            return true;
        }

        private void InitOscQuery()
        {
            // Advertise our OSC JSON and OSC endpoints (OSC JSON to display the silly lil popup in-game)
            _queryRegistrar.Advertise("VRCFT", "_oscjson._tcp", 6970, IPAddress.Loopback);
            _queryRegistrar.Advertise("VRCFT", "_osc._udp", 6969, IPAddress.Loopback);

            _httpHandler?.Dispose();
            _httpHandler = new HttpHandler(6970);
        }

        private bool BindListener()
        {
            _logger.LogTrace("Binding Receiver Client");
            _receiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                // Eventually change this to be random port.
                _receiverClient.Bind(new IPEndPoint(IPAddress.Parse(Address), 6969));

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
                foreach (var line in lines)
                {
                    if (!line.Contains(InPort.ToString()))
                    {
                        continue;
                    }

                    // Get the PID
                    var pid = line.Split(' ').Last().Trim();
                    // Get the process
                    var proc = Process.GetProcessById(int.Parse(pid));
                    _logger.LogError("Port {0} is already bound by {1}", InPort, proc.ProcessName);
                    break;
                }
                
                OnConnectedDisconnected(false);
                return false;
            }
            
            OnConnectedDisconnected(true);
            return true;
        }

        private void StartListenerThread()
        {
            _recvThreadCts?.Cancel();    // In theory, the closure of the client should have already cancelled the token, but just in case

            var newToken = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(ct =>
            {
                if (ct != null)
                {
                    var token = (CancellationToken)ct;
                    while (!token.IsCancellationRequested)
                    {
                        Receive();
                    }
                }
            }, newToken.Token);
            _recvThreadCts = newToken;
        }
        
        private void Receive()
        {
            try
            {
                var bytesReceived = _receiverClient.Receive(_oscBuffer, _oscBuffer.Length, SocketFlags.None);
                var offset = 0;
                var newMsg = new OscMessage(_oscBuffer, bytesReceived, ref offset);
                Array.Clear(_oscBuffer, 0, bytesReceived);
                OnMessageReceived(newMsg);
            }
            catch (Exception e)
            {
                // Ignore as this is most likely a timeout exception
                _logger.LogTrace("Failed to receive message: {0}", e.Message);
            }
        }
        
        private void HandleNewMessage(OscMessage msg)
        {
            switch (msg.Address)
            {
                case "/avatar/change":
                    // If OSCQuery has cached the ip of the VRChat client, we'll use that
                    if (QueryRegistrar.VrchatClientEndpoint != null)
                    {
                        _configParser.ParseCurrentAvatar(QueryRegistrar.VrchatClientEndpoint);
                    }
                    else
                    {
                        _configParser.ParseFromFile(msg.Value as string);
                    }

                    break;
                case "/vrcft/settings/forceRelevant":   // Endpoint for external tools to force VRCFT to send all parameters
                    _paramSupervisor.AllParametersRelevant = (bool)msg.Value;
                    break;
                
                //TODO: Re-think how we handle toggling (if we want to handle it at all at this point)
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

        public void Send(byte[] data, int length)
        {
            _senderClient.Send(data, length, SocketFlags.None);
            OnMessageDispatched();
        }
    }
}