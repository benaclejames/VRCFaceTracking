using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Services;

// Simple service to invoke actions on the UI thread from the Core project.
public class DispatcherService : IDispatcherService
{
    public void Run(Action action)
    {
        var dispatcher = App.MainWindow.DispatcherQueue;
        if (dispatcher == null || dispatcher.HasThreadAccess)
        {
            action();
            return;
        }

        dispatcher.TryEnqueue(action.Invoke);
    }

    public Task RunAsync(Action action)
    {
        // Initialization needs to wait until mutation UI objects are created on the UI thread.
        var dispatcher = App.MainWindow.DispatcherQueue;
        if (dispatcher == null || dispatcher.HasThreadAccess)
        {
            action();
            return Task.CompletedTask;
        }

        var completion = new TaskCompletionSource<object?>();
        if (!dispatcher.TryEnqueue(() =>
            {
                try
                {
                    action();
                    completion.SetResult(null);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }))
        {
            completion.SetException(new InvalidOperationException("Failed to enqueue action on the UI dispatcher."));
        }

        return completion.Task;
    }
}
