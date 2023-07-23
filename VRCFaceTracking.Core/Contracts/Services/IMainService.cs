namespace VRCFaceTracking.Core.Contracts.Services;
public interface IMainService
{
    Action<string, float> ParameterUpdate { get; set; }
    bool AllParametersRelevant { get; set; }
    
    Task Teardown();
    Task InitializeAsync();
}
