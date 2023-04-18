using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryDetailControl : UserControl
{
    public RemoteTrackingModule? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as RemoteTrackingModule;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    private IModuleDataService _moduleDataService;

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(RemoteTrackingModule), typeof(ModuleRegistryDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ModuleRegistryDetailControl()
    {
        InitializeComponent();
        _moduleDataService = App.GetService<IModuleDataService>();
    }
    

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModuleRegistryDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
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
}
