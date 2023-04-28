using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;
using Windows.System;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
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

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        
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

        RecvPort.Value = ViewModel.RecvPort;
        SendPort.Value = ViewModel.SendPort;
        Address.Text = ViewModel.Address;
    }

    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/benaclejames/VRCFaceTracking/issues/new/choose"));
    }

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

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var value = Address.Text;

        // Ensure it passes the regex for an IP address
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^(\d{1,3}\.){3}\d{1,3}$"))
            ViewModel.Address = value;
    }

    private void SendPort_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ViewModel.SendPort = (int)SendPort.Value;

    private void RecvPort_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ViewModel.RecvPort = (int)RecvPort.Value;

    private void AcceptToggle_OnToggled(object sender, RoutedEventArgs e)
    {
        if (dangerAcceptToggle.IsOn)
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

    private void allParamsRelevant_Toggled(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
    private void forceReInitButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void resetVRCFTButton_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void resetVRCAvatarConf_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
