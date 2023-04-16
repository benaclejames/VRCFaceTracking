using System.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
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

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        AvatarViewModel = App.GetService<IAvatarInfo>();
        LibManager = App.GetService<ILibManager>();
        InitializeComponent();
    }
}
