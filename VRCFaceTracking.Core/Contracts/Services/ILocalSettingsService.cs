namespace VRCFaceTracking.Core.Contracts.Services;

public class SavedSettingAttribute : System.Attribute
{
    private readonly string _settingName;
    private readonly object? _defaultValue;
    
    public SavedSettingAttribute(string settingName, object? defaultValue)
    {
        _settingName = settingName;
        _defaultValue = defaultValue;
    }

    public string GetName() => _settingName;
    public object? Default() => _defaultValue;
}
 
public interface ILocalSettingsService
{
    Task<T> ReadSettingAsync<T>(string key, T? defaultValue = default, bool forceLocal = false);

    Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false);

    Task Save(object target);
    Task Load(object target);
}
