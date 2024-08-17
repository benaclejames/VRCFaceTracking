using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Services;

public class ParameterSenderService : BackgroundService
{
    // We probably don't need a queue since we use osc message bundles, but for now, we're keeping it as
    // we might want to allow a way for the user to specify bundle or single message sends in the future
    private static readonly Queue<OscMessage> SendQueue = new();
 
    private readonly OscSendService _sendService;

    public static bool AllParametersRelevantStatic
    {
        get; set;
    }
    public bool AllParametersRelevant
    {
        get => AllParametersRelevantStatic;
        set
        {
            if (AllParametersRelevantStatic == value) return;
            AllParametersRelevantStatic = value;
            SendQueue.Clear();
            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
            {
                parameter.ResetParam(Array.Empty<IParameterDefinition>());
            }
        }
    }
    
    public ParameterSenderService(OscSendService sendService)
    {
        _sendService = sendService;
    }

    public static void Enqueue(OscMessage message) => SendQueue.Enqueue(message);
    public static void Clear() => SendQueue.Clear();
    
    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(10, cancellationToken);

                await UnifiedTracking.UpdateData(cancellationToken);

                // Send all messages in OSCParams.SendQueue
                if (SendQueue.Count <= 0)
                {
                    continue;
                }

                await _sendService.Send(SendQueue.ToArray(), cancellationToken);

                SendQueue.Clear();
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e, scope =>
                {
                    var i = 0;
                    foreach (var msg in SendQueue)
                    {
                        scope.SetExtra($"Address {i}", msg.Address);
                        scope.SetExtra($"Values {i}", msg._meta.ValueLength);
                        scope.SetExtra($"Value 0 {i}", msg.Value);
                        i++;
                    }
                });
            }
        }
    }
}