using VRCFaceTracking_Next.Core.Types;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : OSCParams.BaseParam<float>
    {
        public FloatParameter(Func<UnifiedTrackingData, float> getValueFunc,
            string paramName)
            : base(paramName, getValueFunc) { }
    }
    
    public class AlwaysRelevantParameter<T> : OSCParams.BaseParam<T>
    {
        public AlwaysRelevantParameter(Func<UnifiedTrackingData, T> getValueFunc,
            string paramAddress)
            : base(OSCParams.CurrentVersionPrefix, getValueFunc)
        {
            OscMessage.Address = paramAddress;
            Relevant = true;
        }

        public override IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
        {
            if (newParams != null)
            {
                Relevant = true;
                return new IParameter[] { this };
            }

            Relevant = false;
            return Array.Empty<IParameter>();

        }
    }

    public class ConditionalParameter : IParameter
    {
        private readonly IParameter _param;
        private readonly Func<ConfigParser.Parameter[], bool> _condition;

        public ConditionalParameter(IParameter param, Func<ConfigParser.Parameter[], bool> condition)
        {
            _param = param;
            _condition = condition;
        }

        public IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
        {
            if (!_condition.Invoke(newParams))
            {
                _param.ResetParam(null);    // When we pass null, we want to disable for SURE (even if we're always relevant)
                return Array.Empty<IParameter>();
            }

            return _param.ResetParam(newParams);
        }

        public (string, IParameter)[] GetParamNames() => _param.GetParamNames();

        public bool Deprecated => false;
    }

    public class BoolParameter : OSCParams.BaseParam<bool>
    {
        public BoolParameter(Func<UnifiedTrackingData, bool> getValueFunc,
            string paramName) : base(paramName, getValueFunc)
        {
        }
    }

    // This parameter type will only update parameter 1 if parameter 2 is true
    public class ConditionalBoolParameter : OSCParams.BaseParam<bool>
    {
        private readonly Func<UnifiedTrackingData, (bool, bool)> _conditionalValueFunc;
        
        public ConditionalBoolParameter(Func<UnifiedTrackingData, (bool, bool)> getValueFunc, string paramName) :
            base(paramName, exp => getValueFunc.Invoke(exp).Item1)
        => _conditionalValueFunc = getValueFunc;

        protected override void Process(UnifiedTrackingData exp)
        {
            if (_conditionalValueFunc.Invoke(exp).Item2)
                base.Process(exp);
        }
    }

    public class BinaryParameter : OSCParams.BinaryBaseParameter
    {
        public BinaryParameter(Func<UnifiedTrackingData, float> getValueFunc,
            string paramName) : base(paramName, getValueFunc)
        {
        }
    }

    // EverythingParam, or EpicParam. You choose!
    // Contains a bool, float and binary parameter, all in one class with IParameter implemented.
    public class EParam : IParameter
    {
        private readonly IParameter[] _parameter;

        public EParam(Func<UnifiedTrackingData, float> getValueFunc, string paramName, float minBoolThreshold = 0.5f, bool skipBinaryParamCreation = false)
        {
            if (skipBinaryParamCreation)
            {
                _parameter = new IParameter[]
                {
                    new BoolParameter(exp => getValueFunc.Invoke(exp) < minBoolThreshold, paramName),
                    new FloatParameter(getValueFunc, paramName),
                };
            }
            else
            {
                _parameter = new IParameter[]
                {
                    new BoolParameter(exp => getValueFunc.Invoke(exp) < minBoolThreshold, paramName),
                    new FloatParameter(getValueFunc, paramName),
                    new BinaryParameter(getValueFunc, paramName)
                };
            }
        }

        public EParam(Func<UnifiedTrackingData, Vector2> getValueFunc, string paramName, float minBoolThreshold = 0.5f)
        {
            _parameter = new IParameter[]
            {
                new BoolParameter(exp => getValueFunc.Invoke(exp).x < minBoolThreshold, paramName + "X"),
                new FloatParameter(exp => getValueFunc.Invoke(exp).x, paramName + "X"),
                new BinaryParameter(exp => getValueFunc.Invoke(exp).x, paramName + "X"),

                new BoolParameter(exp => getValueFunc.Invoke(exp).y < minBoolThreshold, paramName + "Y"),
                new FloatParameter(exp => getValueFunc.Invoke(exp).y, paramName + "Y"),
                new BinaryParameter(exp => getValueFunc.Invoke(exp).y, paramName + "Y")
            };
        }

        public IParameter[] ResetParam(ConfigParser.Parameter[] newParams) => _parameter.SelectMany(param => param.ResetParam(newParams)).ToArray();

        public (string, IParameter)[] GetParamNames() => _parameter.SelectMany(param => param.GetParamNames()).ToArray();
        
        public bool Deprecated => false;    // False as our children will handle this
    }
}