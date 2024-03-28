using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Services;

public class OscRecvService : BackgroundService
{
    private Socket _recvSocket;
    private readonly byte[] _recvBuffer = new byte[4096];
    private CancellationToken _stoppingToken;
    private CancellationTokenSource _cts, _linkedToken;
    private readonly ILogger<OscRecvService> _logger;
    public Action<OscMessage> OnMessageReceived = _ => { };
    private readonly IOscTarget _oscTarget;
    private readonly ILocalSettingsService _settingsService;

    public OscRecvService(
        ILogger<OscRecvService> logger,
        IOscTarget oscTarget,
        ILocalSettingsService settingsService
    )
    {
        _logger = logger;
        _cts = new CancellationTokenSource();

        _oscTarget = oscTarget;
        _settingsService = settingsService;
        
        _oscTarget.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not (nameof(IOscTarget.InPort) or nameof(IOscTarget.DestinationAddress)))
            {
                return;
            }

            if (string.IsNullOrEmpty(_oscTarget.DestinationAddress) || _oscTarget.InPort == default)
            {
                return;
            }

            UpdateTarget(new IPEndPoint(IPAddress.Parse(_oscTarget.DestinationAddress), _oscTarget.InPort));
        };
    }

    public async override Task StartAsync(CancellationToken cancellationToken)
    {
        await _settingsService.Load(_oscTarget);
        
        await base.StartAsync(cancellationToken);
    }

    private void UpdateTarget(IPEndPoint endpoint)
    {
        _cts.Cancel();
        _recvSocket?.Close();
        _oscTarget.IsConnected = false;

        _recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        
        try
        {
            _recvSocket.Bind(endpoint);
            _oscTarget.IsConnected = true;
        }
        catch (SocketException ex)
        {
            _logger.LogWarning($"Could not bind to recv endpoint: {endpoint}. {ex.Message}");
        }
        finally
        {
            _cts = new CancellationTokenSource();
            _linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_stoppingToken, _cts.Token);
        }
    }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        
        _linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_stoppingToken, _cts.Token);
        
        while (!_stoppingToken.IsCancellationRequested)
        {
            if (_recvSocket is not { IsBound: true } || _linkedToken.IsCancellationRequested)
            {
                continue;
            }

            try
            {
                var bytesReceived = await _recvSocket.ReceiveAsync(_recvBuffer, _linkedToken.Token);
                var offset = 0;
                var newMsg = await Task.Run(() => OscMessage.TryParseOsc(_recvBuffer, bytesReceived, ref offset), stoppingToken);
                if (newMsg == null)
                {
                    continue;
                }

                OnMessageReceived(newMsg);
            }
            catch (Exception e)
            {
                // We don't care about operation cancellations as they're intentional and carefully controlled
                if (e.GetType() == typeof(OperationCanceledException))
                {
                    continue;
                }
                
                _logger.LogError("Error encountered in OSC Receive thread: {e}", e);
                SentrySdk.CaptureException(e, scope => scope.SetExtra("recvBuffer", _recvBuffer));
            }
        }
    }
}