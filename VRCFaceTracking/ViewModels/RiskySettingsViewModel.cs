using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class RiskySettingsViewModel : ObservableObject
{
    private readonly IMainService _mainService;
    private readonly ILogger<RiskySettingsViewModel> _logger;

    public bool AllParametersRelevant
    {
        get => _mainService.AllParametersRelevant;
        set => _mainService.AllParametersRelevant = value;
    }

    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }
    
    public RiskySettingsViewModel(IMainService mainService, ILogger<RiskySettingsViewModel> logger)
    {
        _mainService = mainService;
        _logger = logger;
    }

    public void ForceReInit()
    {
        if (!Enabled)
            return;
        
        _logger.LogInformation("Reinitializing VRCFT...");
        
        _mainService.Teardown();
        
        _mainService.InitializeAsync();
    }

    public void ResetVRCFT()
    {
        _logger.LogInformation("Resetting VRCFT...");
        
        // Create a file in the VRCFT folder called "reset"
        // This will cause the app to reset on the next launch
        File.Create(Path.Combine(Utils.PersistentDataDirectory, "reset"));
    }

    public void ResetVRCAvatarConf()
    {
        _logger.LogInformation("Resetting VRChat avatar configuration...");
        try
        {
            foreach (var userFolder in Directory.GetDirectories(VRChat.VRCOSCDirectory))
                if (Directory.Exists(userFolder + "\\Avatars"))
                {
                    Directory.Delete(userFolder + "\\Avatars", true);
                }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to reset VRChat avatar configuration! {Message}", e.Message);
        }
    }
}