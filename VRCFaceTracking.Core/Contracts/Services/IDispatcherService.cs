namespace VRCFaceTracking.Core.Contracts.Services;

// Simple interface to allow for easy mocking of the DispatcherService from the Core project
// allowing us to invoke actions on the UI thread from the Core project.
public interface IDispatcherService
{
    public void Run(Action action);

    /// <summary>
    /// Runs an action on the UI dispatcher and completes after the action has run.
    /// </summary>
    public Task RunAsync(Action action);
}
