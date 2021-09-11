using System;
using ParamLib;
using UnityEngine;

namespace VRCFaceTracking.Params
{
    public class FloatEyeParameter : FloatBaseParam, IParameter
    {
        public FloatEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName, bool prioritised = false)
            : base(paramName, prioritised) =>
            MainMod.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public bool IsName(string name) => ParamName == name;
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            MainMod.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                var newValue = getValueFunc.Invoke(eye);
                if (newValue.HasValue) ParamValue = newValue.Value;
            };
        }

        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
        public bool IsName(string name) => X.ParamName == name || Y.ParamName == name;
    }

    public class BoolEyeParameter : BoolBaseParam, IParameter
    {
        public BoolEyeParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : base(paramName) =>
            MainMod.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public bool IsName(string name) => ParamName == name;
    }
}