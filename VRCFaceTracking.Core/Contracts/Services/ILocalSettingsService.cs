namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T> ReadSettingAsync<T>(string key, T? defaultValue = default, bool forceLocal = false);

    Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false);
}
