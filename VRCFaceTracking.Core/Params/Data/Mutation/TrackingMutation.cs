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
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.SDK;
public enum MutationPriority
{
    Preprocessor, 
    None,
    Postprocessor 
}

public enum MutationPropertyType
{
    CheckBox,
    Slider,
    TextBox
}

public class MutationProperty<T> : INotifyPropertyChanged
{
    private object _value;

    public object Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public string Name { get; set; }
    public MutationPropertyType Type { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public abstract partial class TrackingMutation : ObservableObject
{
    public TrackingMutation() 
    {
        Properties = MutationPropertyFactory.CreateProperties(this);
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract MutationPriority Step { get; }
    public List<MutationProperty<object>> Properties { get; }
    public virtual bool IsSaved { get; }

    [ObservableProperty]
    private bool _isActive;

    public ILogger Logger { get; set; }
    public async virtual Task Initialize(UnifiedTrackingData data) => await Task.CompletedTask;
    public abstract void MutateData(ref UnifiedTrackingData data);
    public async virtual Task SaveData(ILocalSettingsService localSettingsService) => await Task.CompletedTask;
    public async virtual Task LoadData(ILocalSettingsService localSettingsService) => await Task.CompletedTask;
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
