using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }
    
    public IAvatarInfo AvatarViewModel
    {
        get;
    }
    
    public ILibManager LibManager
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        AvatarViewModel = App.GetService<IAvatarInfo>();
        LibManager = App.GetService<ILibManager>();
        InitializeComponent();
    }
}
