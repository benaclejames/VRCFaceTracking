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

    public void OnNavigatedTo()
    {
        ViewModel.OnNavigatedTo(null);
    }

    private async void InstallButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected is not InstallableTrackingModule module) return;

        InstallButton.IsEnabled = false;
        InstallButton.Content = "Installing...";

        try
        {
            await _moduleInstaller.InstallRemoteModule(module);
            module.InstallationState = InstallState.Installed;
        }
        finally
        {
            InstallButton.IsEnabled = true;
            InstallButton.Content = "Install";
        }
    }

    private void UninstallButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected is not InstallableTrackingModule module) return;

        _moduleInstaller.UninstallModule(module);
        module.InstallationState = InstallState.AwaitingRestart;
    }

    private async void OpenModulePage_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ViewModel.Selected?.ModulePageUrl is not { Length: > 0 } url) return;

        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher != null)
                await launcher.LaunchUriAsync(new Uri(url));
        }
        catch { }
    }
}
