using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking_Next.ViewModels;

namespace VRCFaceTracking_Next.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
}
