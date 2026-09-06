namespace VRCFaceTracking.Core.Contracts.Services;

public interface IModuleConfigurationService
{
    Task SetInitializationConfig(Guid moduleId, bool eyes, bool expression);
    Task<(bool eyes, bool expression)> GetInitializationConfig(Guid moduleId);
}