using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Services;

public class ParameterSenderService : BackgroundService
{
    private static readonly Queue<OscMessage> SendQueue = new();
 
    private readonly OscQueryService _parameterOutputService;
    public static bool AllParametersRelevant;
    
    public ParameterSenderService(OscQueryService parameterOutputService)
    {
        _parameterOutputService = parameterOutputService;
    }

    public static void Enqueue(OscMessage message) => SendQueue.Enqueue(message);
    public static void Clear() => SendQueue.Clear();
    
    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(10, cancellationToken);

            UnifiedTracking.UpdateData();

            // Send all messages in OSCParams.SendQueue
            if (SendQueue.Count <= 0)
            {
                continue;
            }

            await _parameterOutputService.Send(SendQueue.ToArray());

            SendQueue.Clear();
        }
    }
}