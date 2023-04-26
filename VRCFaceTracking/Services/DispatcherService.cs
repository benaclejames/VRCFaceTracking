using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Services;

// Simple service to invoke actions on the UI thread from the Core project.
public class DispatcherService : IDispatcherService
{
    public void Run(Action action) => App.MainWindow.DispatcherQueue?.TryEnqueue(action.Invoke);
}