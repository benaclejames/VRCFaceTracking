using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Helpers;
using VRCFaceTracking.Helpers;
using VRCFaceTracking.Models;
using Windows.Storage;

namespace VRCFaceTracking.Services;

public class LocalSettingsService : ILocalSettingsService
{
    private const string _defaultApplicationDataFolder = "VRCFaceTracking/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;
    private readonly ILogger<LocalSettingsService> _logger;
    
    private readonly string _localApplicationData = Core.Utils.PersistentDataDirectory;
    private readonly string _applicationDataFolder;
    private readonly string _localSettingsFile;

    private IDictionary<string, object> _settings;

    private bool _isInitialized;
    
    // Save debouncing
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private CancellationTokenSource? _cts = new();
    private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(300);
    
    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options, ILogger<LocalSettingsService> logger)
    {
        _fileService = fileService;
        _options = options.Value;
        _logger = logger;

        _applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
        _localSettingsFile = _options.LocalSettingsFile ?? _defaultLocalSettingsFile;

        _settings = new Dictionary<string, object>();
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        var backupFile = _localSettingsFile + ".bak";
        var mainPath = Path.Combine(_applicationDataFolder, _localSettingsFile);
        var backupPath = Path.Combine(_applicationDataFolder, backupFile);

        var loaded = await Task.Run(() => TestSettingsFileRead(_localSettingsFile));
        if (loaded == null)
        {
            // If our primary settings file is unreadable or corrupt
            _logger.LogWarning("Primary settings is was corrupt");
            loaded = await Task.Run(() => TestSettingsFileRead(backupFile));
            if (loaded != null)
            {
                // But our backup settings file isn't corrupt then restore it
                _logger.LogWarning("Restoring primary settings from session backup");
                Directory.CreateDirectory(_applicationDataFolder);
                File.Copy(backupPath, mainPath, overwrite: true);
            }
        }

        _settings = loaded ?? new Dictionary<string, object>();

        if (loaded == null)
        {
            // Neither file was usable. Restore defaults
            _logger.LogWarning("Restoring default settings");
            await _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }

        if (File.Exists(mainPath))
        {
            // Copy current main file to backup file
            Directory.CreateDirectory(_applicationDataFolder);
            File.Copy(mainPath, backupPath, overwrite: true);
        }

        _isInitialized = true;
    }

    private IDictionary<string, object>? TestSettingsFileRead(string fileName)
    {
        try
        {
            return _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, fileName);
        }
        catch
        {
            return null;
        }
    }

    public async Task<T?> ReadSettingAsync<T>(string key, T? defaultValue = default, bool forceLocal = false)
    {
        if (RuntimeHelper.IsMSIX && !forceLocal)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else
        {
            await InitializeAsync();

            if (_settings != null && _settings.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }

        return defaultValue;
    }

    public async Task SaveSettingAsync<T>(string key, T value, bool forceLocal = false)
    {
        if (RuntimeHelper.IsMSIX && !forceLocal)
        {
            ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
        }
        else
        {
            await InitializeAsync();

            _settings[key] = await Json.StringifyAsync(value);

            await FlushSaveSettings();
            //await _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }
    }

    public async Task Load(object instance)
    {
        var type = instance.GetType();
        var properties = type.GetProperties();
        
        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(typeof(SavedSettingAttribute), false);

            if (attributes.Length <= 0)
            {
                continue;
            }

            var savedSettingAttribute = (SavedSettingAttribute)attributes[0];
            var settingName = savedSettingAttribute.GetName();
            var defaultValue = savedSettingAttribute.Default();

            var setting = await ReadSettingAsync(settingName, defaultValue, savedSettingAttribute.ForceLocal());
            object? convertedSetting;
            try
            {
                convertedSetting = Convert.ChangeType(setting, property.PropertyType);
            }
            catch
            {
                convertedSetting = defaultValue;
            }

            property.SetValue(instance, convertedSetting);
        }
    }

    public async Task Save(object instance)
    {
        var type = instance.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(typeof(SavedSettingAttribute), false);

            if (attributes.Length <= 0)
            {
                continue;
            }

            var savedSettingAttribute = (SavedSettingAttribute)attributes[0];
            var settingName = savedSettingAttribute.GetName();

            await SaveSettingAsync(settingName, property.GetValue(instance), savedSettingAttribute.ForceLocal());
        }
    }

    private async Task FlushSaveSettings()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        
        var cts = new CancellationTokenSource();
        _cts = cts;

        try
        {
            await Task.Delay(Debounce, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Newer save req came in. Skip this one and let the new one do the write.
            return;
        }
        
        await _semaphore.WaitAsync();
        try
        {
            await _fileService.Save(_applicationDataFolder, _localSettingsFile, _settings);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
