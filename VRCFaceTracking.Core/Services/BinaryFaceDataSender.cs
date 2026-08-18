using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Services;

public class BinaryFaceDataSender : IHostedService
{
    private byte[] _dataBuffer = new byte[102];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        UnifiedTracking.OnUnifiedDataUpdated += OnDataUpdated;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        UnifiedTracking.OnUnifiedDataUpdated -= OnDataUpdated;
        return Task.CompletedTask;
    }

    private void OnDataUpdated(UnifiedTrackingData data)
    {
        data.CopyTo(ref _dataBuffer);
        ParameterSenderService.Enqueue(OscMessage.CreateBlob("/tracking/face/v1", _dataBuffer));
    }
}
