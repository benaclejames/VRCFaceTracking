using System.Diagnostics;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;
using Windows.System;

namespace VRCFaceTracking.Views;

public sealed partial class SettingsPage : Page
{
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}" : "Unknown";
        }
    }

    public SettingsViewModel ViewModel
    {
        get;
    }
    
    public OscViewModel OscViewModel
    {
        get;
    }
    
    public UnifiedTrackingMutator CalibrationSettings
    {
        get;
    }
    
    public RiskySettingsViewModel RiskySettingsViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        OscViewModel = App.GetService<OscViewModel>();
        CalibrationSettings = App.GetService<UnifiedTrackingMutator>();
        RiskySettingsViewModel = App.GetService<RiskySettingsViewModel>();
        
        Loaded += OnPageLoaded;
        InitializeComponent();
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        var currentTheme = ViewModel.ElementTheme;
        switch (currentTheme)
        {
            case ElementTheme.Light:
                themeMode.SelectedIndex = 0; break;
            case ElementTheme.Dark:
                themeMode.SelectedIndex = 1; break;
            case ElementTheme.Default:
                themeMode.SelectedIndex = 2; break;
        }
    }

    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    => await Launcher.LaunchUriAsync(new Uri("https://github.com/benaclejames/VRCFaceTracking/issues/new/choose"));
    
    private async void openLocalFolder_OnClick(object sender, RoutedEventArgs e)
        => await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(Utils.PersistentDataDirectory));

    private void themeMode_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();

        if (selectedTheme != null)
        {
            var themeEnum = App.GetEnum<ElementTheme>(selectedTheme);
            if (themeEnum == ElementTheme.Default)
            {
                themeEnum = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
            }

            ViewModel.SwitchThemeCommand.Execute(themeEnum);
        }
    }

    private void AcceptToggle_OnToggled(object sender, RoutedEventArgs e)
    {
        RiskySettingsViewModel.Enabled = dangerAcceptToggle.IsOn;
        if (RiskySettingsViewModel.Enabled)
        {
            // Enable cards
            allParamsRelevant.IsEnabled = true;
            resetVRCFT.IsEnabled = true;
            resetAvatarConfig.IsEnabled = true;
            forceReInit.IsEnabled = true;

            // Enable toggles/buttons
            allParamsRelevantToggle.IsEnabled = true;
            resetVRCFTButton.IsEnabled = true;
            resetVRCAvatarConf.IsEnabled = true;
            forceReInitButton.IsEnabled = true;
        }
        else
        {
            // Disable cards
            allParamsRelevant.IsEnabled = false;
            resetVRCFT.IsEnabled = false;
            resetAvatarConfig.IsEnabled = false;
            forceReInit.IsEnabled = false;

            // Disable toggles/buttons and set them to off
            allParamsRelevantToggle.IsEnabled = false;
            allParamsRelevantToggle.IsOn = false;
            resetVRCFTButton.IsEnabled = false;
            resetVRCAvatarConf.IsEnabled = false;
            forceReInitButton.IsEnabled = false;
        }
    }

    private void forceReInitButton_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ForceReInit();

    private void resetVRCFTButton_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ResetVRCFT();

    private void resetVRCAvatarConf_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ResetVRCAvatarConf();

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => CalibrationSettings.InitializeCalibration();
}
