using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : OSCBaseParams.FloatBaseParam, IParameter
    {
        public FloatParameter(Func<EyeTrackingData, LipTrackingData, float?> getValueFunc,
            string paramName)
            : base(paramName) =>
            UnifiedTrackingData.OnUnifiedDataUpdated += (eye, lip) =>
            {
                //if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public OSCBaseParams.BaseParam[] GetBase() => new OSCBaseParams.BaseParam[] {this};
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, LipTrackingData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new OSCBaseParams.FloatBaseParam(xParamName), new OSCBaseParams.FloatBaseParam(yParamName)) =>
            UnifiedTrackingData.OnUnifiedDataUpdated += (eye, lip) =>
            {
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public XYParameter(Func<EyeTrackingData, Vector2> getValueFunc, string xParamName, string yParamName)
            : this((eye, lip) => getValueFunc.Invoke(eye), xParamName, yParamName)
        {
        }

        public void ResetParam(ConfigParser.Parameter[] newParams) => ResetParams(newParams);

        public OSCBaseParams.BaseParam[] GetBase() => new OSCBaseParams.BaseParam[] {X, Y};
    }

    public class BoolParameter : OSCBaseParams.BoolBaseParam, IParameter
    {
        public BoolParameter(Func<EyeTrackingData, LipTrackingData, bool?> getValueFunc,
            string paramName) : base(paramName) =>
            UnifiedTrackingData.OnUnifiedDataUpdated += (eye, lip) =>
            {
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public BoolParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : this(
            (eye, lip) => getValueFunc.Invoke(eye), paramName)
        {
        }

        public OSCBaseParams.BaseParam[] GetBase()
        {
            return new OSCBaseParams.BaseParam[] {this};
        }
    }

    public class BinaryParameter : OSCBaseParams.BinaryBaseParameter, IParameter
    {
        public BinaryParameter(Func<EyeTrackingData, LipTrackingData, float?> getValueFunc,
            string paramName) : base(paramName)
        {
            UnifiedTrackingData.OnUnifiedDataUpdated += (eye, lip) =>
            {
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };
        }

        public BinaryParameter(Func<EyeTrackingData, float> getValueFunc, string paramName) : this((eye, lip) => getValueFunc.Invoke(eye), paramName)
        {
        }

        public OSCBaseParams.BaseParam[] GetBase()
        {
            OSCBaseParams.BaseParam[] retParams = new OSCBaseParams.BaseParam[_params.Count + 1];
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

        public EParam(Func<EyeTrackingData, LipTrackingData, float?> getValueFunc, string paramName, float minBoolThreshold = 0.5f, bool skipBinaryParamCreation = false)
        {
            var paramLiterals = new List<IParameter>
            {
                new BoolParameter((eye, lip) => getValueFunc.Invoke(eye, lip) < minBoolThreshold, paramName),
                new FloatParameter(getValueFunc, paramName),
            };
            
            if (!skipBinaryParamCreation)
             paramLiterals.Add(new BinaryParameter(getValueFunc, paramName));

            Name = paramName;
            _parameter = paramLiterals.ToArray();
        }

        public EParam(Func<EyeTrackingData, float> getValueFunc, string paramName,
            float minBoolThreshold = 0.5f) : this((eye, lip) => getValueFunc.Invoke(eye), paramName, minBoolThreshold)
        {
        }

        OSCBaseParams.BaseParam[] IParameter.GetBase() => 
            _parameter.SelectMany(p => p.GetBase()).ToArray();

        public void ResetParam(ConfigParser.Parameter[] newParams)
        {
            foreach (var param in _parameter)
                param.ResetParam(newParams);
        }
    }
}