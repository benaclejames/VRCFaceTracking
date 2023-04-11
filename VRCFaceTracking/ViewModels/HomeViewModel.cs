using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking_Next.ViewModels;

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
    
    private ObservableCollection<ModuleViewModule> _modules = new();
    
    public ObservableCollection<ModuleViewModule> Modules
    {
        get => _modules;
        set => SetProperty(ref _modules, value);
    }

    public HomeViewModel(IMainService mainService)
    {
        MainService = mainService;
        Enabled = true;
        Modules = new ObservableCollection<ModuleViewModule>()
        {
            new()
            {
                Name = "Initializing Modules...",
            }
        };
    }
}
