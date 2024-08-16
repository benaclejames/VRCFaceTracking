using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.ViewModels;

public class MutatorViewModel : ObservableRecipient
{
    private readonly UnifiedTrackingMutator _trackingMutator;

    public ObservableCollection<TrackingMutation> Mutations { get; }

    public MutatorViewModel()
    {
        _trackingMutator = App.GetService<UnifiedTrackingMutator>();
        // Initialize the ObservableCollection with the mutations from UnifiedTrackingMutator
        Mutations = new ObservableCollection<TrackingMutation>();
        foreach (var mutation in _trackingMutator._mutations)
            Mutations.Add(mutation);
    }
}
