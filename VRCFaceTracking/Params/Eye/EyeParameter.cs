using System;
using ParamLib;
using UnityEngine;

namespace VRCFaceTracking.Params
{
    public class FloatEyeParameter : FloatBaseParam, IParameter
    {
        public FloatEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName, bool prioritised = false)
            : base(paramName, prioritised) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public string[] GetName() => new[] {ParamName};
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                var newValue = getValueFunc.Invoke(eye);
                if (newValue.HasValue) ParamValue = newValue.Value;
            };
        }

        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
        public string[] GetName() => new[] {X.ParamName, Y.ParamName};

    }

    public class BoolEyeParameter : BoolBaseParam, IParameter
    {
        public BoolEyeParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : base(paramName) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public string[] GetName() => new [] {ParamName};
    }
}