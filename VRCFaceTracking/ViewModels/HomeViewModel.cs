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

    private bool enabled;

    public bool Enabled
    {
        get => enabled;
        set
        {
            MainService.SetEnabled(Enabled);
            SetProperty(ref enabled, value);
        }
    }
    
    private List<ModuleViewModule> modules = new();
    
    public List<ModuleViewModule> Modules
    {
        get => modules;
        set => SetProperty(ref modules, value);
    }

    public HomeViewModel(IMainService mainService)
    {
        MainService = mainService;
        Enabled = true;
        Modules = new List<ModuleViewModule>()
        {
            new ModuleViewModule()
            {
                Name = "meme",
                Images = new List<ImageSource>()
                {
                    new BitmapImage(new Uri("ms-appx:///Assets/QuestPro.png"))
                }
            }
        };
    }
}
