using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ParameterDebugUserControl : UserControl
{
    public ParameterViewModel ViewModel
    {
        get;
    }
    
    public ParameterDebugUserControl()
    {
        ViewModel = App.GetService<ParameterViewModel>();
        InitializeComponent();
        this.DataContext = this;
    }
}