namespace VRCFaceTracking.Core.Contracts.Services;

public class SavedSettingAttribute : Attribute
{
    private readonly string _settingName;
    private readonly object? _defaultValue;
    private readonly bool _forceLocal;

    public SavedSettingAttribute(string settingName, object? defaultValue = default, bool forceLocal = false)
    {
        _settingName = settingName;
        _defaultValue = defaultValue;
        _forceLocal = forceLocal;
    }

    public string GetName() => _settingName;
    public object? Default() => _defaultValue;
    public bool ForceLocal() => _forceLocal;
}
 
public interface ILocalSettingsService
{
    Task<T> ReadSettingAsync<T>(string key, T? defaultValue = default, bool forceLocal = false);

    Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false);

    Task Save(object target);
    Task Load(object target);
}
