using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.DataTypes;

namespace VRCFaceTracking.Core.OSC
{
    public class OscMain : IOSCService
    {
        private static Socket _senderClient, _receiverClient;
        private static CancellationTokenSource _recvThreadCts;
        private readonly ILocalSettingsService _localSettingsService;
        private readonly ILogger _logger;
        private readonly ConfigParser _configParser;
        private readonly IParamSupervisor _paramSupervisor;

        public Action OnMessageDispatched { get; set; }
        public Action<OscMessage> OnMessageReceived { get; set; }
        public Action<bool> OnConnectedDisconnected { get; set; } = b => { };
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
        }

        public int InPort { get; set; }
        public int OutPort { get; set; }
        public string Address { get; set; }


        public async Task LoadSettings()
        {
            var address = await _localSettingsService.ReadSettingAsync<string>("OSCAddress");
            Address = address == default ? "127.0.0.1" : address;

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
            var result = (false, false);

            if (string.IsNullOrWhiteSpace(Address))
                return result;  // Return both false as we cant bind to anything without an address

            result.Item1 = BindListener();
            result.Item2 = BindSender();
            
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

        private bool BindListener()
        {
            _logger.LogTrace("Binding Receiver Client");
            _receiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                _receiverClient.Bind(new IPEndPoint(IPAddress.Parse(Address), InPort));

                StartListenerThread();
            }
            catch
            {
                _logger.LogError("Failed to bind to port {0}", InPort);
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
            ThreadPool.QueueUserWorkItem(new WaitCallback(ct =>
            {
                var token = (CancellationToken)ct;
                while (!token.IsCancellationRequested)
                    Recv();
            }), newToken.Token);
            _recvThreadCts = newToken;
        }

        private static byte[] buffer = new byte[2048];
        private void Recv()
        {
            try
            {
                var bytesReceived = _receiverClient.Receive(buffer, buffer.Length, SocketFlags.None);
                var offset = 0;
                var newMsg = new OscMessage(buffer, bytesReceived, ref offset);
                Array.Clear(buffer, 0, bytesReceived);
                OnMessageReceived(newMsg);
            }
            catch (Exception e)
            {
                // Ignore as this is most likely a timeout exception
                _logger.LogTrace("Failed to receive message: {0}", e.Message);
                return;
            }
        }
        
        private void HandleNewMessage(OscMessage msg)
        {
            switch (msg.Address)
            {
                case "/avatar/change":
                    _configParser.ParseNewAvatar(msg.Value as string);
                    break;
                case "/vrcft/settings/forceRelevant":   // Endpoint for external tools to force vrcft to send all parameters
                    _paramSupervisor.AllParametersRelevant = (bool)msg.Value;
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

        public void Send(byte[] data, int length)
        {
            _senderClient.Send(data, length, SocketFlags.None);
            OnMessageDispatched();
        }
    }
}