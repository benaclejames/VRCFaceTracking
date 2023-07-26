namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T> ReadSettingAsync<T>(string key, T? defaultValue = default);

    Task SaveSettingAsync<T>(string key, T value);
}
