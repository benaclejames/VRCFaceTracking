using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking_Next.ViewModels;

namespace VRCFaceTracking_Next.Views;

public sealed partial class DevicesPage : Page
{
    public DevicesViewModel ViewModel
    {
        get;
    }

    public DevicesPage()
    {
        ViewModel = App.GetService<DevicesViewModel>();
        InitializeComponent();
    }
}
