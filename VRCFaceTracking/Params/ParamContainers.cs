using System;
using System.Collections.Generic;
using System.Linq;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : OSCParams.FloatBaseParam, IParameter
    {
        public FloatParameter(Func<UnifiedExpressionsData, float?> getValueFunc,
            string paramName)
            : base(paramName) =>
            UnifiedTracking.OnUnifiedDataUpdated += (expression) =>
            {
                //if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(expression);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public OSCParams.BaseParam[] GetBase() => new OSCParams.BaseParam[] {this};
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<UnifiedExpressionsData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new OSCParams.FloatBaseParam(xParamName), new OSCParams.FloatBaseParam(yParamName)) =>
            UnifiedTracking.OnUnifiedDataUpdated += (expression) =>
            {
                var value = getValueFunc.Invoke(expression);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public void ResetParam(ConfigParser.Parameter[] newParams) => ResetParams(newParams);

        public OSCParams.BaseParam[] GetBase() => new OSCParams.BaseParam[] {X, Y};
    }

    public class BoolParameter : OSCParams.BoolBaseParam, IParameter
    {
        public BoolParameter(Func<UnifiedExpressionsData, bool?> getValueFunc,
            string paramName) : base(paramName) =>
            UnifiedTracking.OnUnifiedDataUpdated += (expression) =>
            {
                var value = getValueFunc.Invoke(expression);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public OSCParams.BaseParam[] GetBase()
        {
            return new OSCParams.BaseParam[] {this};
        }
    }

    public class BinaryParameter : OSCParams.BinaryBaseParameter, IParameter
    {
        public BinaryParameter(Func<UnifiedExpressionsData, float?> getValueFunc,
            string paramName) : base(paramName)
        {
            UnifiedTracking.OnUnifiedDataUpdated += (expression) =>
            {
                var value = getValueFunc.Invoke(expression);
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
        private readonly string Name;

        public EParam(Func<UnifiedExpressionsData, float?> getValueFunc, string paramName, float minBoolThreshold = 0.5f, bool skipBinaryParamCreation = false)
        {
            var paramLiterals = new List<IParameter>
            {
                new BoolParameter((expression) => getValueFunc.Invoke(expression) < minBoolThreshold, paramName),
                new FloatParameter(getValueFunc, paramName),
            };
            
            if (!skipBinaryParamCreation)
             paramLiterals.Add(new BinaryParameter(getValueFunc, paramName));

            Name = paramName;
            _parameter = paramLiterals.ToArray();
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