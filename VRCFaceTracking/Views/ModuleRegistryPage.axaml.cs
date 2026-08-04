using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class ModuleRegistryPage : UserControl
{
    private ModuleRegistryViewModel ViewModel => (ModuleRegistryViewModel)DataContext!;
    private readonly ModuleInstaller _moduleInstaller;

    public ModuleRegistryPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<ModuleRegistryViewModel>();
        _moduleInstaller = Ioc.Default.GetRequiredService<ModuleInstaller>();
    }

    public async void OnNavigatedTo() => await ViewModel.OnNavigatedTo();

    private async void ModuleSelection_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {   
        if (ViewModel.Selected is not InstallTrackedTrackingModule module) return;
        InstallButton.IsVisible = module.InstallationState != InstallState.Installed;
        UninstallButton.IsVisible = module.InstallationState == InstallState.Installed;
        InstallButton.Content = "Install";
        InstallButton.IsEnabled = true;
        if (module.InstallationState != InstallState.AwaitingRestart)
        {
            UninstallButton.IsEnabled = true;
            UninstallButton.Content =  "Uninstall";
        }
    }
    private async void InstallButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected is not InstallTrackedTrackingModule module) return;
        InstallButton.IsEnabled = false;
        InstallButton.Content = "Installing...";

        try
        {
            await _moduleInstaller.InstallRemoteModule(module.TrackingModuleMetadata);
            module.InstallationState = InstallState.Installed;
        }
        finally
        {
            InstallButton.Content = "Installed.";
        }
    }

    private async void UninstallButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected is not InstallTrackedTrackingModule module) return;

        UninstallButton.IsEnabled = false;
        await _moduleInstaller.UninstallModule(module.TrackingModuleMetadata);
        await ViewModel.OnNavigatedTo();
    }

    private async void OpenModulePage_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected?.TrackingModuleMetadata.ModulePageUrl is not { Length: > 0 } url) return;
        
        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher != null)
                await launcher.LaunchUriAsync(new Uri(url));
        }
        catch { }
    }
}
