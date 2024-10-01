using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.ViewModels;

public class MutatorViewModel : ObservableRecipient
{
    private readonly UnifiedTrackingMutator _trackingMutator;

    public ObservableCollection<TrackingMutation> Mutations { get; }

    public MutatorViewModel()
    {
        _trackingMutator = App.GetService<UnifiedTrackingMutator>();
        Mutations = _trackingMutator._mutations;
    }
}
