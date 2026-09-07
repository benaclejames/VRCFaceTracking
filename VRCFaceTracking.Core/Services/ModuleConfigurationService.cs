using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

public class ModuleConfigurationService(ILocalSettingsService settingsService, IModuleDataService dataService) : IModuleConfigurationService
{
    private readonly Dictionary<Guid, ModuleConfigEntry> _cachedConfigs = new();
    
    public Task SaveModule(ModuleConfigEntry module) =>
        settingsService.Save(module, Prefix(module.Id));

    public async Task<ModuleConfigEntry?> LoadModule(Guid id)
    {
        if (_cachedConfigs.TryGetValue(id, out var cachedConfig)) return cachedConfig;
        
        var module = dataService.GetInstalledModules().FirstOrDefault(x => x.ModuleId == id);
        if (module == null) return null;

        var newConfig = new ModuleConfigEntry(module, this);
        await settingsService.Load(newConfig, Prefix(id));

        return newConfig;
    }

    private static string Prefix(Guid id) => $"Module:{id}:";
}
