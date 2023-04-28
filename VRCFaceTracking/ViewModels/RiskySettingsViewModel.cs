using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class RiskySettingsViewModel : ObservableObject
{
    private readonly IMainService _mainService;

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
    
    public RiskySettingsViewModel(IMainService mainService)
    {
        _mainService = mainService;
    }

    public void ForceReInit()
    {
        _mainService.Teardown();
        
        _mainService.InitializeAsync();
    }
}