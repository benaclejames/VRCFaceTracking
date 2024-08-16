using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

public class MutationProperty : INotifyPropertyChanged
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

public abstract class TrackingMutation : INotifyPropertyChanged
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract MutationPriority Step { get; }
    public abstract List<MutationProperty> Properties { get; }
    public virtual bool IsSaved { get; }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

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
