using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

public enum MutationPropertyType
{
    CheckBox,
    Slider,
    TextBox
}

public interface IMutationComponent
{
    public string Name { get; }
}

public class MutationProperty : IMutationComponent, INotifyPropertyChanged
{
    private object _value;
    private readonly Action<object> _updateField;

    public MutationProperty(string name, 
                            object value, 
                            MutationPropertyType type, 
                            Action<object> updateField, 
                            float min, 
                            float max)
    {
        Name = name;
        _value = value;
        Type = type;
        _updateField = updateField;
        Min = min;
        Max = max;
    }

    public float Min { get; }
    public float Max { get; }

    public object Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                _updateField?.Invoke(_value);
            }
        }
    }

    public string Name { get; }
    public MutationPropertyType Type { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class MutationRangeProperty : IMutationComponent, INotifyPropertyChanged
{
    private float _item1;
    private float _item2;
    private readonly Action<(float, float)> _updateField;

    public MutationRangeProperty(string name, 
                                 float item1, 
                                 float item2, 
                                 Action<(float, float)> updateField, 
                                 float min, 
                                 float max)
    {
        Name = name;
        _item1 = item1;
        _item2 = item2;
        _updateField = updateField;
        Min = min;
        Max = max;
    }

    public float Min { get; }
    public float Max { get; }

    public float Item1
    {
        get => _item1;
        set
        {
            if (_item1 != value)
            {
                _item1 = value;
                OnPropertyChanged(nameof(Item1));
                _updateField?.Invoke((_item1, _item2));
            }
        }
    }

    public float Item2
    {
        get => _item2;
        set
        {
            if (_item2 != value)
            {
                _item2 = value;
                OnPropertyChanged(nameof(Item2));
                _updateField?.Invoke((_item1, _item2));
            }
        }
    }

    public string Name { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class MutationAction : IMutationComponent, ICommand
{
    public string Name { get; }
    private readonly Action _execute;

    public MutationAction(string name, Action execute)
    {
        Name = name;
        _execute = execute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => true; // Adjust logic as needed

    public void Execute(object parameter) => Task.Run(() => _execute());
}
