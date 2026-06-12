using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.ViewModels;

public class MutatorViewModel : ObservableRecipient
{
    public ObservableCollection<TrackingMutation> Mutations { get; } = new();

    public MutatorViewModel(UnifiedTrackingMutator trackingMutator)
    {
        Mutations.AddRange(trackingMutator._mutations);

        trackingMutator._mutations.CollectionChanged += OnSourceCollectionChanged;
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.NewItems != null)
                foreach (TrackingMutation m in e.NewItems)
                    Mutations.Add(m);

            if (e.OldItems != null)
                foreach (TrackingMutation m in e.OldItems)
                    Mutations.Remove(m);
        });
    }
}
