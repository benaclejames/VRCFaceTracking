using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Services;

public class OscRecvService : BackgroundService
{
    private readonly ILogger<OscRecvService> _logger;
    private readonly IOscTarget _oscTarget;
    private readonly ILocalSettingsService _settingsService;

    private Socket _recvSocket;
    private readonly byte[] _recvBuffer = new byte[4096];

    private CancellationTokenSource _cts, _linkedToken;
    private CancellationToken _stoppingToken;

    public Action<OscMessage> OnMessageReceived = _ => { };

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
            if (args.PropertyName == nameof(_oscTarget.IsConnected))
                return;
            
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(_oscTarget);
    
            if (!Validator.TryValidateObject(_oscTarget, context, validationResults, validateAllProperties: true))
            {
                var errorMessages = string.Join(Environment.NewLine, validationResults.Select(v => v.ErrorMessage));
                _logger.LogWarning($"{errorMessages} Reverting to default.");
                if (_oscTarget.DestinationAddress != "127.0.0.1")
                {
                    _oscTarget.DestinationAddress = "127.0.0.1";
                }
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

    public IPEndPoint UpdateTarget(IPEndPoint endpoint)
    {
        if (!Equals(endpoint.Address, IPAddress.Loopback))
        {
            _logger.LogError("Cannot bind to non-loopback IP");
            return null;
        }
        
        _logger.LogInformation($"Updating osc recv target to {endpoint}");
        _cts.Cancel();
        _recvSocket?.Close();
        _oscTarget.IsConnected = false;

        _recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _recvSocket.Bind(endpoint);
            _oscTarget.IsConnected = true;
            _logger.LogInformation($"Successfully connected to remote endpoint at {_recvSocket.LocalEndPoint}");
            return (IPEndPoint)_recvSocket.LocalEndPoint;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not bind to recv endpoint: {endpoint}. {ex.Message}");
        }
        finally
        {
            _cts = new CancellationTokenSource();
            _linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_stoppingToken, _cts.Token);
        }

        return null;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        _linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_stoppingToken, _cts.Token);

        while (!_stoppingToken.IsCancellationRequested)
        {
            if (_linkedToken.IsCancellationRequested || _recvSocket is not { IsBound: true })
            {
                continue;
            }

            try
            {
                if (_recvSocket.Available > 0)
                {
                    var bytesReceived =
                        await _recvSocket.ReceiveAsync(_recvBuffer, SocketFlags.None, _linkedToken.Token);
                    var offset = 0;
                    var newMsg = OscMessage.TryParseOsc(_recvBuffer, bytesReceived, ref offset);
                    if (newMsg == null)
                    {
                        continue;
                    }

                    OnMessageReceived(newMsg);
                }
                else
                {
                    await Task.Delay(100, _linkedToken.Token);
                }
            }
            catch (Exception e)
            {
                // We don't care about operation cancellations as they're intentional and carefully controlled
                if (e.GetType() == typeof(OperationCanceledException) || e.GetType() == typeof(TaskCanceledException))
                {
                    continue;
                }

                _logger.LogError("Error encountered in OSC Receive thread: {e}", e);
                SentrySdk.CaptureException(e, scope => scope.SetExtra("recvBuffer", _recvBuffer));
            }
        }
    }
}
