using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ParametersPage : Page
{
    public ParametersViewModel ViewModel
    {
        get;
    }

    public ParametersPage()
    {
        ViewModel = App.GetService<ParametersViewModel>();
        InitializeComponent();
    }
}
