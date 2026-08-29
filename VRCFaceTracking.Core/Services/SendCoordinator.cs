using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Services;

// Helper to fire OSC send **when** new data is sent back to main process.
// This replaces the lazy "check for changes at 10hz" loop we were using before
public sealed class SendCoordinator(ParameterSenderService sender, ILogger<SendCoordinator> logger)
{
    private const int CoalesceWindowMs = 10;    // How long we're willing to wait for all modules once one has replied

    private readonly object _lock = new();
    private readonly HashSet<int> _fresh = new();
    private readonly HashSet<int> _required = new();
    private Timer? _timer;
    private int _emitting;

    public void RegisterModule(int port)
    {
        lock (_lock) _required.Add(port);
    }

    public void UnregisterModule(int port)
    {
        lock (_lock)
        {
            _required.Remove(port);
            _fresh.Remove(port);
        }
    }

    public void NotifyReply(int port)
    {
        var now = Environment.TickCount64;
        int delay = -1;
        bool emitNow = false;

        lock (_lock)
        {
            _fresh.Add(port);

            var haveQuorum = _required.Count == 0 || _required.IsSubsetOf(_fresh);

            if (haveQuorum)
            {
                // Everyone has responded. Stop the timer and send now
                _timer?.Dispose();
                _timer = null;

                emitNow = true;
            }
            else if (_timer == null)
            {
                // Wait for the others
                var target = now + CoalesceWindowMs;
                delay = (int)(target - now);
                if (delay < 1) delay = 1;
            }

            if (delay > 0)
            {
                _timer = new Timer(Emit, null, delay, Timeout.Infinite);
            }
        }

        if (emitNow) Emit();
    }

    private void Emit(object? _ = null)
    {
        // Dont emit while we're already emitting
        if (Interlocked.CompareExchange(ref _emitting, 1, 0) != 0) return;

        lock (_lock)
        {
            _fresh.Clear();
        }

        Task.Run(async () =>
        {
            try
            {
                UnifiedTracking.UpdateDataSync();
                await sender.FlushAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.LogError(e, "OSC emit failed");
            }
            finally
            {
                Volatile.Write(ref _emitting, 0);
            }
        });
    }
}
