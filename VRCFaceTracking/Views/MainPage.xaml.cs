using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
