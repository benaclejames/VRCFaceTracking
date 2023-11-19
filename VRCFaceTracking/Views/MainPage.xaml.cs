using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }
    
    public INavigationService NavigationService
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        NavigationService = App.GetService<INavigationService>();

        InitializeComponent();
    }

    private void NoModuleButton_Click(object sender, RoutedEventArgs e) => NavigationService.NavigateTo(typeof(ModuleRegistryViewModel).FullName!);
}
