using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryDetailControl : UserControl
{
    public RemoteTrackingModule? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as RemoteTrackingModule;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    private readonly IModuleDataService _moduleDataService;
    private readonly ModuleInstaller _moduleInstaller;
    private readonly ILibManager _libManager;

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(RemoteTrackingModule), typeof(ModuleRegistryDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ModuleRegistryDetailControl()
    {
        InitializeComponent();
        _moduleDataService = App.GetService<IModuleDataService>();
        _moduleInstaller = App.GetService<ModuleInstaller>();
        _libManager = App.GetService<ILibManager>();
    }
    

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModuleRegistryDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        switch (ListDetailsMenuItem!.InstallationState)
        {
            case InstallState.NotInstalled or InstallState.Outdated:
            {
                var path = await _moduleInstaller.InstallRemoteModule(ListDetailsMenuItem!);
                if (path != null)
                {
                    ListDetailsMenuItem!.InstallationState = InstallState.Installed;
                    _libManager.Initialize();
                    InstallButton.Content = "Uninstall";
                    InstallButton.IsEnabled = true;
                }
                break;
            }
            case InstallState.Installed:
            {
                InstallButton.Content = "Install";
                InstallButton.IsEnabled = true;
                _libManager.TeardownAllAndReset();
                _moduleInstaller.UninstallModule(ListDetailsMenuItem!);
                ListDetailsMenuItem!.InstallationState = InstallState.NotInstalled;
                _libManager.Initialize();
                break;
            }
        }
        
    }

    private async void RatingControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Attempt to get our rating from the API.
        var rating = await _moduleDataService.GetMyRatingAsync(ListDetailsMenuItem!);
        if (rating > 0) // If we already rated this module, set the rating control to that value.
        {
            RatingControl.PlaceholderValue = rating;
            RatingControl.Value = rating;
            RatingControl.Caption = "Your Rating";
        }
        else // Otherwise, set the rating control to the average rating.
        {
            if (ListDetailsMenuItem!.Rating > 0)
                RatingControl.PlaceholderValue = ListDetailsMenuItem!.Rating;

            RatingControl.Caption = $"{ListDetailsMenuItem!.Ratings} ratings";
        }
    }

    private async void RatingControl_OnValueChanged(RatingControl sender, object args)
    {
        RatingControl.Caption = "Your Rating";
        
        await _moduleDataService.SetMyRatingAsync(ListDetailsMenuItem!, (int)RatingControl.Value);
    }

    private void InstallButton_OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set our state depending on our assigned installation state.
        switch (ListDetailsMenuItem!.InstallationState)
        {
            case InstallState.NotInstalled:
                InstallButton.Content = "Install";
                break;
            case InstallState.Installed:
                InstallButton.Content = "Uninstall";
                break;
            case InstallState.Outdated:
                InstallButton.Content = "Update";
                break;
        }
        InstallButton.IsEnabled = true;
    }
}
