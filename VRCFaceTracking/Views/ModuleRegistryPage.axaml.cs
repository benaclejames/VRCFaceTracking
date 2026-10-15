using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Strings;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class ModuleRegistryPage : UserControl, INotifyNavigated
{
    private ModuleRegistryViewModel ViewModel => (ModuleRegistryViewModel)DataContext!;
    private readonly ModuleInstaller _moduleInstaller;
    private readonly ILibManager _libManager;
    private readonly ILocalSettingsService _settingsService;
    private bool _suppressStateChange;

    public ModuleRegistryPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<ModuleRegistryViewModel>();
        _moduleInstaller = Ioc.Default.GetRequiredService<ModuleInstaller>();
        _libManager = Ioc.Default.GetRequiredService<ILibManager>();
        _settingsService = Ioc.Default.GetRequiredService<ILocalSettingsService>();
    }

    public async void OnNavigatedTo() => await ViewModel.OnNavigatedTo();

    private async void ModuleSelection_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {   
        if (ViewModel.Selected is not InstallTrackedTrackingModule module) return;
        var isInstalled = module.InstallationState != InstallState.NotInstalled;
        InstallButton.IsVisible = module.InstallationState != InstallState.Installed;
        UninstallButton.IsVisible = module.InstallationState == InstallState.Installed;
        ModuleStateComboBox.IsVisible = isInstalled;
        InstallButton.Content = "Install";
        InstallButton.IsEnabled = true;
        if (module.InstallationState != InstallState.AwaitingRestart)
        {
            UninstallButton.IsEnabled = true;
            UninstallButton.Content =  "Uninstall";
        }

        _suppressStateChange = true;
        ModuleStateComboBox.SelectedIndex = (int)module.State;
        _suppressStateChange = false;
        UpdateStateHint(module);
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

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Install from .zip",
            AllowMultiple = false,
            FileTypeFilter = [
                new FilePickerFileType("Zip Files")
                {
                    Patterns = (IReadOnlyList<string>)
                    [
                        "*.zip"
                    ],
                    AppleUniformTypeIdentifiers = (IReadOnlyList<string>)
                    [
                        "public.zip"
                    ],
                    MimeTypes = (IReadOnlyList<string>)
                        [
                            "application/zip",
                            "application/x-zip",
                            "application/x-zip-compressed",
                            "application/zip-compressed",
                            "multipart/x-zip"
                        ]
                    
                }
            ]
        });

        try
        {
            foreach (var file in files)
            {
                await _moduleInstaller.InstallLocalModule(file.Path.LocalPath);
            }
        }
        finally
        {
            await _libManager.Initialize();
        }
    }

    private void UpdateStateHint(InstallTrackedTrackingModule module)
    {
        var applied = _libManager.AppliedModuleStates.TryGetValue(module.ModuleKey, out var a) ? a : (ModuleEnabledState?)null;
        ModuleEnabledHint.IsVisible = applied.HasValue && applied.Value != module.State;
    }

    private async void ModuleStateComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (_suppressStateChange || ViewModel.Selected is not InstallTrackedTrackingModule module)
        {
            return;
        }
        if (ModuleStateComboBox.SelectedIndex < 0)
        {
            return;
        }

        var state = (ModuleEnabledState)ModuleStateComboBox.SelectedIndex;
        module.State = state;

        var applied = _libManager.AppliedModuleStates.TryGetValue(module.ModuleKey, out var a) ? a : (ModuleEnabledState?)null;
        module.UpdateStateBadge(applied, Localize);
        ModuleEnabledHint.IsVisible = applied.HasValue && applied.Value != state;

        await SaveStateAsync(module, state);
    }

    private async Task SaveStateAsync(InstallTrackedTrackingModule module, ModuleEnabledState state)
    {
        var allSettings = await _settingsService.ReadSettingAsync(
            VRCFaceTracking.Core.Utils.ModuleStateSettingsKey,
            new Dictionary<string, ModuleEnabledState>());
        allSettings[module.ModuleKey] = state;
        await _settingsService.SaveSettingAsync(VRCFaceTracking.Core.Utils.ModuleStateSettingsKey, allSettings);
    }

    private static string Localize(ModuleEnabledState state) => state switch
    {
        ModuleEnabledState.Enabled => VRCFaceTracking.Strings.Resources.ModuleStateText_Enabled,
        ModuleEnabledState.Disabled => VRCFaceTracking.Strings.Resources.ModuleStateText_Disabled,
        ModuleEnabledState.EyesOnly => VRCFaceTracking.Strings.Resources.ModuleStateText_EyesOnly,
        ModuleEnabledState.FaceOnly => VRCFaceTracking.Strings.Resources.ModuleStateText_FaceOnly,
        _ => VRCFaceTracking.Strings.Resources.ModuleStateText_Enabled
    };
}
