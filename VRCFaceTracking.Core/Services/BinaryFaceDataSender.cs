using System.Buffers;
using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Services;

public class BinaryFaceDataSender : IHostedService
{
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
        var buffer = ArrayPool<byte>.Shared.Rent(UnifiedTrackingData.SerializedLength);
        data.CopyTo(ref buffer);
        ParameterSenderService.Enqueue(OscMessage.CreateBlob("/tracking/face/v1", buffer, UnifiedTrackingData.SerializedLength));
    }
}
