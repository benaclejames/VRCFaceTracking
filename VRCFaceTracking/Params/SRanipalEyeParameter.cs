using System;
using ParamLib;
using UnityEngine;

namespace VRCFaceTracking.Params
{
    public class FloatEyeParameter : FloatBaseParam, IParameter
    {
        public FloatEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName, bool prioritised = false)
            : base(paramName, prioritised) =>
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);
    }

    public class XYEyeParameter : XYParam, IParameter
    {
        public XYEyeParameter(Func<EyeTrackingData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) =>
            {
                var newValue = getValueFunc.Invoke(eye);
                if (newValue.HasValue) ParamValue = newValue.Value;
            };
        }

        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
    }

    public class BoolEyeParameter : BoolBaseParam, IParameter
    {
        public BoolEyeParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : base(paramName) =>
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);
    }
}