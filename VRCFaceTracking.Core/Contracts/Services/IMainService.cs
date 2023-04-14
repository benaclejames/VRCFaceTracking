namespace VRCFaceTracking.Core.Contracts.Services;
public interface IMainService
{
    void Teardown();
    void SetEnabled(bool newEnabled);
    Task InitializeAsync(Action<Action> dispatcherRun);
}
