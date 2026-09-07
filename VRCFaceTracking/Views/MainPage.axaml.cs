using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Contracts;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class MainPage : UserControl, INotifyNavigated
{
    private MainViewModel ViewModel => (MainViewModel)DataContext!;
    
    public MainPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
    }

    public void OnNavigatedTo() => ViewModel.OnNavigatedTo();
}