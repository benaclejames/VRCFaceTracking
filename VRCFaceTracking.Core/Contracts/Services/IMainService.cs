namespace VRCFaceTracking.Core.Contracts.Services;
public interface IMainService
{
    void Teardown();
    Task InitializeAsync(Action<Action> dispatcherRun);
}
