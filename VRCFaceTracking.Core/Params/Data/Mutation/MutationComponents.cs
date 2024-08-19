using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VRCFaceTracking.SDK;

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
