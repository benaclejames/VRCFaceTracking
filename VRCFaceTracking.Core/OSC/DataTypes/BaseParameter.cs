using System.Text.RegularExpressions;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.OSC.DataTypes;

public static class QueueController
{
    public static Queue<OscMessageMeta> SendQueue = new();
    public static bool AlwaysRelevantDebug = false;
}

public class BaseParam<T> : IParameter where T : struct
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

    private T _lastValue;

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

    private void Enqueue() => QueueController.SendQueue.Enqueue(OscMessage._meta);

    protected readonly OscMessage OscMessage;

    public BaseParam(string name, Func<UnifiedTrackingData, T> getValueFunc, bool sendOnLoad = false)
    {
        _paramName = name;
        _regex = new Regex(@"(?<!(v\d+))(/" + _paramName + @")$|^(" + _paramName + @")$");
        _getValueFunc = getValueFunc;
        OscMessage = new OscMessage(DefaultPrefix + name, typeof(T));
        _sendOnLoad = sendOnLoad;
    }

    public virtual IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
    {
        if (QueueController.AlwaysRelevantDebug)
        {
            Relevant = true;
            OscMessage.Address = DefaultPrefix + _paramName;

            return new IParameter[] { this };
        }

        var compatibleParam = newParams.FirstOrDefault(param =>
            _regex.IsMatch(param.name)
            && param.input.Type == typeof(T));

        if (compatibleParam != null)
        {
            Relevant = true;
            OscMessage.Address = compatibleParam.input.address;
        }
        else
        {
            Relevant = false;
            OscMessage.Address = DefaultPrefix + _paramName;
        }

        return Relevant ? new IParameter[] { this } : Array.Empty<IParameter>();
    }

    public (string, IParameter)[] GetParamNames() => new[] { (_paramName, (IParameter)this) };

    public bool Deprecated => !_paramName.StartsWith(CurrentVersionPrefix);

    protected virtual void Process(UnifiedTrackingData data) => ParamValue = _getValueFunc.Invoke(data);

    ~BaseParam()
    {
        // Not sure if this is actually needed, but it's good practice
        UnifiedTracking.OnUnifiedDataUpdated -= Process;
    }
}