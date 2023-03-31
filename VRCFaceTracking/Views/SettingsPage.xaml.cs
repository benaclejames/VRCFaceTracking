using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking_Next.Helpers;
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

    private async void RecvPort_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        var oldValue = args.OldValue;
        var newValue = args.NewValue;

        if (oldValue == newValue) return;

        ViewModel.RecvPort = (int)newValue;
        var success = await ViewModel.ReInitOsc();
        if (!success.Item1)
        {
            // If Recv not Success
            //TODO: Mark box as red-ish or add exclamation mark or something
        }
    }
    private async void SendPort_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        var oldValue = args.OldValue;
        var newValue = args.NewValue;

        if (oldValue == newValue) return;

        ViewModel.SendPort = (int)newValue;
        var success = await ViewModel.ReInitOsc();
        if (!success.Item2)
        {
            // If Send not Success
            //TODO: Mark box as red-ish or add exclamation mark or something
        }
    }
}
