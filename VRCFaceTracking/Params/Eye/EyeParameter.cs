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

    public class BinaryEyeParameter : BinaryParam, IParameter
    {
        public BinaryEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName) : base
        (
            new BoolBaseParam(paramName + "1"), 
            new BoolBaseParam(paramName + "2"), 
            new BoolBaseParam(paramName + "4"), 
            new BoolBaseParam(paramName + "8")
        ) => 
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                Q1.ParamValue = (int)(getValueFunc.Invoke(eye) * 15 + .5) % 2 == 1;
                Q2.ParamValue = (int)(getValueFunc.Invoke(eye) * 15 + .5)/2 % 2 == 1;
                Q3.ParamValue = (int)(getValueFunc.Invoke(eye) * 15 + .5)/4 % 2 == 1;
                Q4.ParamValue = (int)(getValueFunc.Invoke(eye) * 15 + .5)/8 % 2 == 1;
            };
        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
        public string[] GetName() => new[] { Q1.ParamName, Q2.ParamName, Q3.ParamName, Q4.ParamName };
    }
}