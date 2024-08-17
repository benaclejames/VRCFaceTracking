using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.SDK;
public enum MutationPriority
{
    Preprocessor, 
    None,
    Postprocessor 
}

public abstract partial class TrackingMutation : ObservableObject
{
    public abstract string Name { get; }
    [JsonIgnore]
    public abstract string Description { get; }
    public abstract MutationPriority Step { get; }
    [JsonIgnore]
    public ObservableCollection<IMutationComponent> Components { get; set; }
    public virtual bool IsSaved { get; } = false;

    [ObservableProperty]
    private bool _isActive;

    [JsonIgnore]
    public ILogger Logger { get; set; }
    public async virtual Task Initialize(UnifiedTrackingData data) => await Task.CompletedTask;
    public abstract void MutateData(ref UnifiedTrackingData data);
    public void CreateProperties() => Components = MutationComponentFactory.CreateComponents(this);
    public static TrackingMutation[] GetImplementingMutations(bool ordered = true)
    {
        var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(type => type.IsSubclassOf(typeof(TrackingMutation)));

        List<TrackingMutation> mutations = new List<TrackingMutation>();
        foreach (var t in types)
        {
            var mutation = (TrackingMutation)Activator.CreateInstance(t);
            mutations.Add(mutation);
        }

        if (ordered)
        {
            mutations.OrderBy(m => m.Step);
        }

        return mutations.ToArray();
    }
}
