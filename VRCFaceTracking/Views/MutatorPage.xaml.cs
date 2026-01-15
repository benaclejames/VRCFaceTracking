using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Data.Mutation;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class MutatorPage : Page
{
    public MutatorViewModel ViewModel
    {
        get;
    }

    public MutatorPage()
    {
        var trackingMutator = App.GetService<UnifiedTrackingMutator>();
        ViewModel = App.GetService<MutatorViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private async void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggle && toggle.DataContext is TrackingMutation mutation)
        {
            await mutation.Save();
        }
    }
}
