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

/// <summary>
/// Read-only informational row in a mutation settings panel.
/// </summary>
public class MutationInfo : IMutationComponent
{
    public MutationInfo(string name)
    {
        Name = name;
    }

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
    public string ButtonText { get; }
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    private readonly Action<Action>? _dispatch;

    public MutationAction(
        string name,
        Action execute,
        string? buttonText = null,
        Func<bool>? canExecute = null,
        Action<Action>? dispatch = null)
    {
        Name = name;
        ButtonText = buttonText ?? name;
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
        _dispatch = dispatch;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute();

    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        Task.Run(() => _execute());
    }

    public void Refresh()
    {
        Dispatch(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
    }

    private void Dispatch(Action action)
    {
        if (_dispatch != null)
        {
            _dispatch(action);
            return;
        }

        action();
    }
}

/// <summary>
/// Read-only status row whose value is supplied by a callback and refreshed on demand.
/// </summary>
public class MutationStatus : IMutationComponent, INotifyPropertyChanged
{
    private readonly Func<string> _getValue;
    private readonly Action<Action>? _dispatch;

    public MutationStatus(string name, Func<string> getValue, Action<Action>? dispatch = null)
    {
        Name = name;
        _getValue = getValue;
        _dispatch = dispatch;
    }

    public string Name { get; }

    public string Value => _getValue();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Refresh()
    {
        Dispatch(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
    }

    private void Dispatch(Action action)
    {
        if (_dispatch != null)
        {
            _dispatch(action);
            return;
        }

        action();
    }
}

/// <summary>
/// Combined status row and command button for stateful mutation actions.
/// </summary>
public class MutationStatusAction : IMutationComponent, ICommand, INotifyPropertyChanged
{
    private readonly Func<string> _getStatus;
    private readonly Func<string> _getButtonText;
    private readonly Func<bool> _canExecute;
    private readonly Action _execute;
    private readonly Action<Action>? _dispatch;

    public MutationStatusAction(
        string name,
        Func<string> getStatus,
        Func<string> getButtonText,
        Func<bool> canExecute,
        Action execute,
        Action<Action>? dispatch = null)
    {
        Name = name;
        _getStatus = getStatus;
        _getButtonText = getButtonText;
        _canExecute = canExecute;
        _execute = execute;
        _dispatch = dispatch;
    }

    public string Name { get; }

    public string Status => _getStatus();

    public string ButtonText => _getButtonText();

    public event EventHandler? CanExecuteChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool CanExecute(object? parameter) => _canExecute();

    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        Task.Run(() => _execute());
    }

    public void Refresh()
    {
        Dispatch(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonText)));
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    private void Dispatch(Action action)
    {
        if (_dispatch != null)
        {
            _dispatch(action);
            return;
        }

        action();
    }
}
