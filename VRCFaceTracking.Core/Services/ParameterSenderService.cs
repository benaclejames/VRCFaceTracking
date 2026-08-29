using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Services;

public class ParameterSenderService
{
    // We probably don't need a queue since we use osc message bundles, but for now, we're keeping it as
    // we might want to allow a way for the user to specify bundle or single message sends in the future
    private static readonly Queue<OscMessage> SendQueue = new();

    private readonly OscSendService _sendService;
    private readonly UnifiedTrackingMutator _mutator; // We don't use this but we do want DI to run its constructor

    private readonly SemaphoreSlim _flushLock = new(1, 1);

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
            foreach (var parameter in UnifiedTracking.AllParameters)
            {
                parameter.ResetParam(Array.Empty<IParameterDefinition>());
            }
        }
    }

    public ParameterSenderService(OscSendService sendService, UnifiedTrackingMutator mutator)
    {
        _sendService = sendService;
        _mutator = mutator;
    }

    public static void Enqueue(OscMessage message) => SendQueue.Enqueue(message);
    public static void Clear() => SendQueue.Clear();

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (SendQueue.Count == 0) return;

        await _flushLock.WaitAsync(cancellationToken);
        try
        {
            if (SendQueue.Count == 0) return;
            var messages = SendQueue.ToArray();
            SendQueue.Clear();
            await _sendService.Send(messages, cancellationToken);
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
        }
        finally
        {
            _flushLock.Release();
        }
    }
}
