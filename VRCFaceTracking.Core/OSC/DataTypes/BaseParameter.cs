using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.OSC.DataTypes;

public class ParamSupervisor : IParamSupervisor
{
    public static readonly Queue<OscMessage> SendQueue = new();
 
    private readonly IDispatcherService _dispatcherService;
    
    public static bool AllParametersRelevantStatic { get; set; }

    public bool AllParametersRelevant
    {
        get => AllParametersRelevantStatic;
        set
        {
            if (AllParametersRelevantStatic == value) return;
            AllParametersRelevantStatic = value;
            SendQueue.Clear();
            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                parameter.ResetParam(Array.Empty<IParameterDefinition>());
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        _dispatcherService.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    public ParamSupervisor(IDispatcherService dispatcherService)
    {
        _dispatcherService = dispatcherService;
    }
}

public class BaseParam<T> : Parameter where T : struct
{
    private const string DefaultPrefix = "/avatar/parameters/";
    protected const string CurrentVersionPrefix = "v2/";

    private readonly Func<UnifiedTrackingData, T> _getValueFunc;

    private readonly string _paramName;
    private readonly Regex _regex;

    private bool _relevant;
    private bool _sendOnLoad;

    public bool Relevant
    {
        get => _relevant;
        protected set
        {
            if (_sendOnLoad && value) Enqueue();

            // If we're irrelevant or we don't have a getValueFunc, we don't need to do anything
            if (_relevant == value) return;

            _relevant = value;

            if (_getValueFunc == null) return;

            if (value)
                UnifiedTracking.OnUnifiedDataUpdated += Process;
            else
                UnifiedTracking.OnUnifiedDataUpdated -= Process;
        }
    }

    private T? _lastValue;

    public T ParamValue
    {
        get => (T)OscMessage.Value;
        set
        {
            if (value.Equals(_lastValue)) return;

            OscMessage.Value = value;
            _lastValue = value;
            Enqueue();
        }
    }

    private void Enqueue() => ParamSupervisor.SendQueue.Enqueue(OscMessage);

    protected readonly OscMessage OscMessage;

    public BaseParam(string name, Func<UnifiedTrackingData, T> getValueFunc, bool sendOnLoad = false)
    {
        _paramName = name;
        _regex = new Regex(@"(?<!(v\d+))(/" + _paramName + @")$|^(" + _paramName + @")$");
        _getValueFunc = getValueFunc;
        OscMessage = new OscMessage(DefaultPrefix + name, typeof(T));
        _sendOnLoad = sendOnLoad;
    }

    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        if (ParamSupervisor.AllParametersRelevantStatic)
        {
            Relevant = true;
            OscMessage.Address = DefaultPrefix + _paramName;

            return new Parameter[] { this };
        }

        var compatibleParam = newParams.FirstOrDefault(param =>
            _regex.IsMatch(param.Name)
            && param.Type == typeof(T));

        if (compatibleParam != null)
        {
            Relevant = true;
            OscMessage.Address = compatibleParam.Address;
        }
        else
        {
            Relevant = false;
            OscMessage.Address = DefaultPrefix + _paramName;
        }

        return Relevant ? new Parameter[] { this } : Array.Empty<Parameter>();
    }

    public override (string, Parameter)[] GetParamNames() => new[] { (_paramName, (Parameter)this) };

    public override bool Deprecated => !_paramName.StartsWith(CurrentVersionPrefix);

    protected virtual void Process(UnifiedTrackingData data) => ParamValue = _getValueFunc.Invoke(data);

    ~BaseParam()
    {
        // Not sure if this is actually needed, but it's good practice
        UnifiedTracking.OnUnifiedDataUpdated -= Process;
    }
}