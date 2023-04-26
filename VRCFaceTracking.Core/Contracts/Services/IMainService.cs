namespace VRCFaceTracking.Core.Contracts.Services;
public interface IMainService
{
    void Teardown();
    Task InitializeAsync();
    Action<string, float> ParameterUpdate { get; set; }
}
