using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class HomeViewModel : ObservableRecipient
{
    private IMainService MainService
    {
        get;
    }

    private bool _enabled;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            MainService.SetEnabled(Enabled);
            SetProperty(ref _enabled, value);
        }
    }

    public HomeViewModel(IMainService mainService)
    {
        MainService = mainService;
        Enabled = true;
    }
}
