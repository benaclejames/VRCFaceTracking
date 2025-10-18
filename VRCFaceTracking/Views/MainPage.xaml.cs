using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Models;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;
using Windows.System;

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

    public ObservableCollection<LoggingContext> HomeLogs => OutputPageLogger.ErrorLogs;

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        NavigationService = App.GetService<INavigationService>();

        InitializeComponent();
    }

    private void NoModuleButton_Click(object sender, RoutedEventArgs e) => NavigationService.NavigateTo(typeof(ModuleRegistryViewModel).FullName!);

    private async void ModuleWikiLink_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://docs.vrcft.io/docs/hardware"));
}
