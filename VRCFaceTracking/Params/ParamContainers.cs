using System;
using System.Linq;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : OSCParams.BaseParam<float>
    {
        public FloatParameter(Func<UnifiedTrackingData, float> getValueFunc,
            string paramName)
            : base(paramName, getValueFunc) { }
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
        public bool Deprecated => false;    // False as our children will handle this
    }
}