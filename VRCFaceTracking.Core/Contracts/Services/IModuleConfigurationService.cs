using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface IModuleConfigurationService
{
    Task SaveModule(ModuleConfigEntry module);
    Task<ModuleConfigEntry?> LoadModule(Guid id);
}