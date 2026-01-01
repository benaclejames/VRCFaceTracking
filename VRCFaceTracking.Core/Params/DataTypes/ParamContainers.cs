using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Params.DataTypes;

// Extension of BaseParam with ResetParam overridden so that it always parses as relevant
public class AlwaysRelevantParameter<T> : BaseParam<T> where T : struct
{
    public AlwaysRelevantParameter(Func<UnifiedTrackingData, T> getValueFunc,
        string paramAddress)
        : base(CurrentVersionPrefix, getValueFunc)
    {
        OscMessage.Address = paramAddress;
        Relevant = true;
    }
        
    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        Relevant = true;
        return new Parameter[] { this };
    }
}

public class NativeParameter<T> : AlwaysRelevantParameter<T> where T : struct
{
    private readonly Func<IParameterDefinition[], bool> _condition;
        
    public NativeParameter(Func<UnifiedTrackingData, T> getValueFunc, Func<IParameterDefinition[], bool> condition, string paramAddress) : base(getValueFunc, paramAddress)
    {
        _condition = condition;
    }
        
    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        if (_condition.Invoke(newParams))
        {
            return base.ResetParam(newParams);
        }

        Relevant = false;
        return Array.Empty<Parameter>();

    }
}

// This parameter type will only update parameter 1 if parameter 2 is true
public class ConditionalBoolParameter : BaseParam<bool>
{
    private readonly Func<UnifiedTrackingData, (bool, bool)> _conditionalValueFunc;
        
    public ConditionalBoolParameter(Func<UnifiedTrackingData, (bool, bool)> getValueFunc, string paramName) :
        base(paramName, exp => getValueFunc.Invoke(exp).Item1)
    {
        _conditionalValueFunc = getValueFunc;
    }

    // Override deprecated, since we never really want to deprecate these parameters
    public override bool Deprecated => false;

    protected override void Process(UnifiedTrackingData exp)
    {
        if (_conditionalValueFunc.Invoke(exp).Item2)
        {
            base.Process(exp);
        }
    }
}

// EverythingParam, or EpicParam. You choose!
// Contains a bool, float and binary parameter, all in one class with IParameter implemented.
public class EParam : Parameter
{
    private readonly Parameter[] _parameter;

    public EParam(string paramName, Func<UnifiedTrackingData, float> getValueFunc, float minBoolThreshold = 0.5f,
        bool skipBinaryParamCreation = false)
    {
        if (skipBinaryParamCreation)
        {
            _parameter = new Parameter[]
            {
                new BaseParam<bool>(paramName, exp => getValueFunc.Invoke(exp) < minBoolThreshold),
                new BaseParam<float>(paramName, getValueFunc),
            };
        }
        else
        {
            _parameter = new Parameter[]
            {
                new BaseParam<bool>(paramName, exp => getValueFunc.Invoke(exp) < minBoolThreshold),
                new BaseParam<float>(paramName, getValueFunc),
                new BinaryBaseParameter(paramName, getValueFunc)
            };
        }
    }

    public EParam(string paramName, Func<UnifiedTrackingData, Vector2> getValueFunc, float minBoolThreshold = 0.5f)
    {
        _parameter = new Parameter[]
        {
            new BaseParam<bool>(paramName + "X", exp => getValueFunc.Invoke(exp).x < minBoolThreshold),
            new BaseParam<float>(paramName + "X", exp => getValueFunc.Invoke(exp).x),
            new BinaryBaseParameter(paramName + "X", exp => getValueFunc.Invoke(exp).x),

            new BaseParam<bool>(paramName + "Y", exp => getValueFunc.Invoke(exp).y < minBoolThreshold),
            new BaseParam<float>(paramName + "Y", exp => getValueFunc.Invoke(exp).y),
            new BinaryBaseParameter(paramName + "Y", exp => getValueFunc.Invoke(exp).y)
        };
    }

    public override Parameter[] ResetParam(IParameterDefinition[] newParams) => _parameter.SelectMany(param => param.ResetParam(newParams)).ToArray();

    public override (string, Parameter)[] GetParamNames() => _parameter.SelectMany(param => param.GetParamNames()).ToArray();
}