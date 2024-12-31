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

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public enum MutationPriority
{
    Preprocessor, 
    None,
    Postprocessor 
}

public abstract partial class TrackingMutation
{
    public abstract string Name { get; }
    [JsonIgnore]
    public abstract string Description { get; }
    public abstract MutationPriority Step { get; }
    [JsonIgnore]
    public ObservableCollection<IMutationComponent> Components { get; set; }
    public virtual bool IsSaved { get; } = false;

    public virtual bool IsActive { get; set; }

    [JsonIgnore]
    public ILogger Logger { get; set; }
    public virtual void Initialize(UnifiedTrackingData data) { }
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
            mutations.Sort((a, b) => a.Step.CompareTo(b.Step));
        }

        return mutations.ToArray();
    }
}
