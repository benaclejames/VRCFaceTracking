using System;
using System.Collections.Generic;
using System.Linq;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : OSCParams.FloatBaseParam, IParameter
    {
        public FloatParameter(Func<UnifiedTrackingData, float?> getValueFunc,
            string paramName)
            : base(paramName) =>
            UnifiedTracking.OnUnifiedDataUpdated += exp =>
            {
                //if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(exp);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public OSCParams.BaseParam[] GetBase() => new OSCParams.BaseParam[] {this};
    }

    public class BoolParameter : OSCParams.BoolBaseParam, IParameter
    {
        public BoolParameter(Func<UnifiedTrackingData, bool?> getValueFunc,
            string paramName) : base(paramName) =>
            UnifiedTracking.OnUnifiedDataUpdated += exp =>
            {
                var value = getValueFunc.Invoke(exp);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public OSCParams.BaseParam[] GetBase()
        {
            return new OSCParams.BaseParam[] {this};
        }
    }

    public class ConditionalBoolParameter : OSCParams.BoolBaseParam, IParameter
    {
        public ConditionalBoolParameter(Func<UnifiedTrackingData, (bool?, bool?)> getValueFunc, string paramName) : base(paramName) =>
            UnifiedTracking.OnUnifiedDataUpdated += exp =>
            {
                if (getValueFunc.Invoke(exp).Item2.HasValue && getValueFunc.Invoke(exp).Item2.Value)
                {
                    var value = getValueFunc.Invoke(exp).Item1;
                    if (value.HasValue)
                        ParamValue = value.Value;
                }
            };

        public OSCParams.BaseParam[] GetBase()
        {
            return new OSCParams.BaseParam[] { this };
        }
    }

    public class BinaryParameter : OSCParams.BinaryBaseParameter, IParameter
    {
        public BinaryParameter(Func<UnifiedTrackingData, float?> getValueFunc,
            string paramName) : base(paramName)
        {
            UnifiedTracking.OnUnifiedDataUpdated += exp =>
            {
                var value = getValueFunc.Invoke(exp);
                if (value.HasValue)
                    ParamValue = value.Value;
            };
        }

        public OSCParams.BaseParam[] GetBase()
        {
            OSCParams.BaseParam[] retParams = new OSCParams.BaseParam[_params.Count + 1];
            // Merge _params.Values and _negativeParam
            Array.Copy(_params.Values.ToArray(), retParams, _params.Count);
            retParams[_params.Count] = _negativeParam;
            return retParams;
        }
    }

    // EverythingParam, or EpicParam. You choose!
    // Contains a bool, float and binary parameter, all in one class with IParameter implemented.
    public class EParam : IParameter
    {
        private readonly IParameter[] _parameter;

        public EParam(Func<UnifiedTrackingData, float?> getValueFunc, string paramName, float minBoolThreshold = 0.5f, bool skipBinaryParamCreation = false)
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

        public EParam(Func<UnifiedTrackingData, Vector2?> getValueFunc, string paramName, float minBoolThreshold = 0.5f)
        {
            _parameter = new IParameter[]
            {
                new BoolParameter(exp => getValueFunc.Invoke(exp).Value.x < minBoolThreshold, paramName + "X"),
                new FloatParameter(exp => getValueFunc.Invoke(exp).Value.x, paramName + "X"),
                new BinaryParameter(exp => getValueFunc.Invoke(exp).Value.x, paramName + "X"),

                new BoolParameter(exp => getValueFunc.Invoke(exp).Value.y < minBoolThreshold, paramName + "Y"),
                new FloatParameter(exp => getValueFunc.Invoke(exp).Value.y, paramName + "Y"),
                new BinaryParameter(exp => getValueFunc.Invoke(exp).Value.y, paramName + "Y")
            };
        }

        OSCParams.BaseParam[] IParameter.GetBase() => 
            _parameter.SelectMany(p => p.GetBase()).ToArray();

        public void ResetParam(ConfigParser.Parameter[] newParams)
        {
            foreach (var param in _parameter)
                param.ResetParam(newParams);
        }
    }
}