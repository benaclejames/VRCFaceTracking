using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.ViewModels;

public partial class RiskySettingsViewModel : ObservableObject
{
    private readonly IMainService _mainService;
    private readonly ParameterSenderService _parameterSenderService;
    private readonly ILogger<RiskySettingsViewModel> _logger;

    [ObservableProperty] private bool _enabled;

    public bool AllRelevantDebug
    {
        get => ParameterSenderService.AllParametersRelevant;
        set => ParameterSenderService.AllParametersRelevant = value;
    }

    public RiskySettingsViewModel(
        IMainService mainService, 
        ParameterSenderService parameterSendService, 
        ILogger<RiskySettingsViewModel> logger
        )
    {
        _mainService = mainService;
        _parameterSenderService = parameterSendService;
        _logger = logger;
    }

    /// <summary>
    /// If something goes very wrong some time during init, this can be used to force retry.
    /// Most modules likely will not like this.
    /// </summary>
    public void ForceReInit()
    {
        _logger.LogInformation("Reinitializing VRCFT...");
        
        _mainService.Teardown();
        
        _mainService.InitializeAsync();
    }

    /// <summary>
    /// Worse case, will delete all persistent data stored by VRCFT. The same data is erased upon uninstall too.
    /// </summary>
    public void ResetVRCFT()
    {
        _logger.LogInformation("Resetting VRCFT...");
        
        // Create a file in the VRCFT folder called "reset"
        // This will cause the app to reset on the next launch
        File.Create(Path.Combine(Utils.PersistentDataDirectory, "reset"));
    }
}