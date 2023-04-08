using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking.OSC
{
    public class OscMain : IOSCService
    {
        private static Socket SenderClient, ReceiverClient;
        private static Thread _receiveThread;
        private static CancellationTokenSource _recvThreadCts;
        private readonly ILocalSettingsService _localSettingsService;
        private readonly ILogger _logger;
        private readonly ConfigParser _configParser;

        public OscMain(ILocalSettingsService localSettingsService, ILoggerFactory loggerFactory, ConfigParser configParser)
        {
            _localSettingsService = localSettingsService;
            _configParser = configParser;
            _logger = loggerFactory.CreateLogger("OSC");
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
            await LoadSettings();
            var result = (false, false);

            if (string.IsNullOrWhiteSpace(Address))
                return result;  // Return both false as we cant bind to anything without an address

            result.Item1 = BindListener();
            result.Item2 = BindSender();

            await Task.CompletedTask;
            return result;
        }

        private bool BindSender()
        {
            _logger.LogTrace("Binding Sender Client");
            SenderClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                SenderClient.Connect(new IPEndPoint(IPAddress.Parse(Address), OutPort));
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool BindListener()
        {
            _logger.LogTrace("Binding Receiver Client");
            ReceiverClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                ReceiverClient.Bind(new IPEndPoint(IPAddress.Parse(Address), InPort));

                StartListenerThread();
            }
            catch
            {
                return false;
            }

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

        private void Recv()
        {
            byte[] buffer = new byte[2048];
            try
            {
                ReceiverClient.Receive(buffer, buffer.Length, SocketFlags.None);
            }
            catch (Exception)
            {
                // Ignore as this is most likely a timeout exception
                return;
            }
            var newMsg = new OscMessage(buffer);
            switch (newMsg.Address)
            {
                case "/avatar/change":
                    _configParser.ParseNewAvatar((string)newMsg.Value);
                    break;
                case "/avatar/parameters/EyeTrackingActive":
                    if (UnifiedLibManager.EyeStatus != ModuleState.Uninitialized)
                    {
                        if (!(bool)newMsg.Value)
                            UnifiedLibManager.EyeStatus = ModuleState.Idle;
                        else UnifiedLibManager.EyeStatus = ModuleState.Active;
                    }
                    break;
                case "/avatar/parameters/LipTrackingActive":
                case "/avatar/parameters/ExpressionTrackingActive":
                    if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized)
                    {
                        if (!(bool)newMsg.Value)
                            UnifiedLibManager.ExpressionStatus = ModuleState.Idle;
                        else UnifiedLibManager.ExpressionStatus = ModuleState.Active;
                    }
                    break;
            }
        }

        public void Send(byte[] data, int length) => SenderClient.Send(data, length, SocketFlags.None);
    }
}