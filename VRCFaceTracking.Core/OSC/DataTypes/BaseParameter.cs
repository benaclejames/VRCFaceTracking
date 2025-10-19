using System.Text.RegularExpressions;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core.OSC.DataTypes;

public class BaseParam<T> : Parameter where T : struct
{
    private const string DefaultPrefix = "/avatar/parameters/";
    protected const string CurrentVersionPrefix = "v2/";

    private readonly Func<UnifiedTrackingData, T> _getValueFunc;

    private readonly string _paramName;
    private readonly Regex _regex;

    private bool _relevant;
    private readonly bool _sendOnLoad;

    public bool Relevant
    {
        get => _relevant;
        protected set
        {
            if (_sendOnLoad && value)
            {
                Enqueue();
            }

            // If we're irrelevant or we don't have a getValueFunc, we don't need to do anything
            if (_relevant == value)
            {
                return;
            }

            _relevant = value;

            if (_getValueFunc == null)
            {
                return;
            }

            if (value)
            {
                UnifiedTracking.OnUnifiedDataUpdated += Process;
            }
            else
            {
                UnifiedTracking.OnUnifiedDataUpdated -= Process;
            }
        }
    }

    private T? _lastValue;

    public T ParamValue
    {
        get => (T)OscMessages[0].Value;
        set
        {
            if (value.Equals(_lastValue))
            {
                return;
            }

            foreach (var msg in OscMessages)
            {
                msg.Value = value;
            }
            _lastValue = value;
            Enqueue();
        }
    }

    private void Enqueue()
    {
        foreach (var msg in OscMessages)
        {
            ParameterSenderService.Enqueue(msg);
        }
    }

    protected readonly List<OscMessage> OscMessages = new();

    public BaseParam(string name, Func<UnifiedTrackingData, T> getValueFunc, bool sendOnLoad = false)
    {
        _paramName = name;
        _regex = new Regex(@"(?<!(v\d+))(/" + _paramName + ")$|^(" + _paramName + ")$");
        _getValueFunc = getValueFunc;
        OscMessages.Add(new OscMessage(DefaultPrefix + name, typeof(T)));
        _sendOnLoad = sendOnLoad;
    }

    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        OscMessages.Clear();
        OscMessages.Add(new OscMessage(DefaultPrefix + _paramName, typeof(T)));
        
        // If we're forcing all parameter relevant, ignore these checks entirely and assume relevancy of the default address for this param.
        if (ParameterSenderService.AllParametersRelevantStatic)
        {
            Relevant = true;
            return new Parameter[] { this };
        }
        
        var compatibleParams = newParams.Where(param =>
            _regex.IsMatch(param.Address)
            && param.Type == typeof(T)).ToList();
        
        // Ensures that the FT prefix is always included, if it's not already there
        // Messages will be sent to two addresses in cases where the param uses a non-FT prefix: the default, and the FT prefix
        // Params that aren't used at all won't be sent even to the default FT address
        if (compatibleParams.Any())
        {
            OscMessages.Clear();
            OscMessages.AddRange(compatibleParams.Select(p => new OscMessage(p.Address, typeof(T))));

            if (!compatibleParams.Any(param => param.Address.Contains("/FT/")))
            {
                var ftAddress = DefaultPrefix + "FT/" + _paramName;
                OscMessages.Add(new OscMessage(ftAddress, typeof(T)));
            }

            Relevant = true;
        }
        else
        {
            Relevant = false;
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