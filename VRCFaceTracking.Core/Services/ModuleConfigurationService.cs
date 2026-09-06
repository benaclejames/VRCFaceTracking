using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Services;

public class ModuleConfigurationService(ILocalSettingsService settingsService) : IModuleConfigurationService
{
    private string Key(Guid moduleId, string key) => $"ModuleConfig:{moduleId}:{key}";
    
    private async Task SaveModuleSetting<T>(Guid moduleId, string key, T setting) => 
        await settingsService.SaveSettingAsync(Key(moduleId, key), setting);
    
    private async Task<T> GetModuleSetting<T>(Guid moduleId, string key, T? defaultValue = default) =>
        await settingsService.ReadSettingAsync(Key(moduleId, key), defaultValue);
    
    
    public async Task SetInitializationConfig(Guid moduleId, bool eyes, bool expression)
    {
        byte value = 0x0;
        if (eyes) value |= 0x1;
        if (expression) value |= 0x2;

        await SaveModuleSetting(moduleId, "Initialization", value);
    }

    public async Task<(bool eyes, bool expression)> GetInitializationConfig(Guid moduleId)
    {
        var value = await GetModuleSetting<byte>(moduleId, "Initialization", 0x3);
        return ((value & 0x1) == 0x1, (value & 0x2) == 0x2);
    }
}