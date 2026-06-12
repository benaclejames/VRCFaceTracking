using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class MainPage : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
    }
}