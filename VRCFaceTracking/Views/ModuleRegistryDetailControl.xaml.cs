using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryDetailControl
{
    public InstallableTrackingModule? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as InstallableTrackingModule;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    private readonly IModuleDataService _moduleDataService;
    private readonly ModuleInstaller _moduleInstaller;
    private readonly ILibManager _libManager;
    private readonly MainViewModel _mainViewModel;

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(TrackingModuleMetadata), typeof(ModuleRegistryDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ModuleRegistryDetailControl()
    {
        InitializeComponent();
        _moduleDataService = App.GetService<IModuleDataService>();
        _moduleInstaller = App.GetService<ModuleInstaller>();
        _libManager = App.GetService<ILibManager>();
        _mainViewModel = App.GetService<MainViewModel>();
    }
    

    private static async void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ModuleRegistryDetailControl control)
        {
            return;
        }

        control.ForegroundElement.ChangeView(0, 0, 1);
        control.InstallButton.IsEnabled = true;
        switch (control.ListDetailsMenuItem!.InstallationState)
        {
            case InstallState.NotInstalled:
                control.InstallButton.Content = "Install";
                break;
            case InstallState.Installed:
                control.InstallButton.Content = "Uninstall";
                break;
            case InstallState.Outdated:
                control.InstallButton.Content = "Update";
                break;
            case InstallState.AwaitingRestart:
                control.InstallButton.Content = "Please Restart VRCFT";
                control.InstallButton.IsEnabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
            
        // Attempt to get our rating from the API.
        var rating = await control._moduleDataService.GetMyRatingAsync(control.ListDetailsMenuItem!);
        if (rating.HasValue) // If we already rated this module, set the rating control to that value.
        {
            control.RatingControl.PlaceholderValue = rating.Value;
            control.RatingControl.Value = rating.Value;
            control.RatingControl.Caption = "Your Rating";
        }
        else // Otherwise, set the rating control to the average rating.
        {
            control.RatingControl.ClearValue(RatingControl.ValueProperty);
            control.RatingControl.ClearValue(RatingControl.PlaceholderValueProperty);

            if (control.ListDetailsMenuItem!.Rating > 0)
                control.RatingControl.PlaceholderValue = control.ListDetailsMenuItem!.Rating;

            control.RatingControl.Caption = $"{control.ListDetailsMenuItem!.Ratings} ratings";
        }
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        switch (ListDetailsMenuItem!.InstallationState)
        {
            case InstallState.NotInstalled or InstallState.Outdated:
            {
                _libManager.TeardownAllAndResetAsync();
                var path = await _moduleInstaller.InstallRemoteModule(ListDetailsMenuItem!);
                if (path != null)
                {
                    ListDetailsMenuItem!.InstallationState = InstallState.Installed;
                    await _moduleDataService.IncrementDownloadsAsync(ListDetailsMenuItem!);
                    ListDetailsMenuItem!.Downloads++;
                    _libManager.Initialize();
                    InstallButton.Content = "Uninstall";
                    InstallButton.IsEnabled = true;
                    _mainViewModel.NoModulesInstalled = false;
                }
                break;
            }
            case InstallState.Installed:
            {
                InstallButton.Content = "Please Restart VRCFT";
                InstallButton.IsEnabled = false;
                _libManager.TeardownAllAndResetAsync();
                _moduleInstaller.MarkModuleForDeletion(ListDetailsMenuItem!);
                _libManager.Initialize();
                break;
            }
        }
        
    }

    private async void RatingControl_OnValueChanged(RatingControl sender, object args)
    {
        RatingControl.Caption = "Your Rating";
        
        await _moduleDataService.SetMyRatingAsync(ListDetailsMenuItem!, (int)RatingControl.Value);
    }
}
