using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking_Next.ViewModels;
using Windows.System;

namespace VRCFaceTracking_Next.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public string Version
        {
            get
            {
                var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
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

    private async void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var value = ((TextBox)sender).Text;

        // Ensure it passes the regex for an IP address
        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^(\d{1,3}\.){3}\d{1,3}$"))
        {
            await ViewModel.SetAddress(value);
        }
    }

    private async void SendPort_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => await ViewModel.SetSendPort((int)args.NewValue);

    private async void RecvPort_OnValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => await ViewModel.SetRecvPort((int)args.NewValue);
}
