using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Services;

public class BinaryFaceDataSender(OscQueryService oscqService) : IHostedService
{
    private byte[] _blobBuffer = new byte[UnifiedTrackingData.SerializedLength];
    private OscMessage? _blobMessage;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _blobMessage = OscMessage.CreateBlob("/tracking/face/v1", _blobBuffer, _blobBuffer.Length);
        
        UnifiedTracking.OnUnifiedDataUpdated += OnDataUpdated;
        oscqService.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(OscQueryService.AvatarParameters)) return;
            SubscribeConditional((sender as OscQueryService)!.AvatarParameters);
        };
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        UnifiedTracking.OnUnifiedDataUpdated -= OnDataUpdated;
        return Task.CompletedTask;
    }

    private void SubscribeConditional(IEnumerable<Parameter> parameters)
    {
        var shouldBeSubscribed = !parameters.Any();
        var isAlreadySubscribed = UnifiedTracking.OnUnifiedDataUpdated.GetInvocationList().Any(x => x.Target == this);
        
        if (shouldBeSubscribed && !isAlreadySubscribed)
        {
            UnifiedTracking.OnUnifiedDataUpdated += OnDataUpdated;
        }
        else if (!shouldBeSubscribed && isAlreadySubscribed)
        {
            UnifiedTracking.OnUnifiedDataUpdated -= OnDataUpdated;
        }
    }
    
    private void OnDataUpdated(UnifiedTrackingData data)
    {
        if (_blobMessage == null) return;
        data.CopyTo(ref _blobBuffer);
        ParameterSenderService.Enqueue(_blobMessage);
    }
}
