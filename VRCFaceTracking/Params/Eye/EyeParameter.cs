using System;
using ParamLib;
using UnityEngine;

namespace ParamLib
{
    public class BinaryParam
    {
        public BoolBaseParam Q1, Q2, Q3, Q4;

        protected bool ParamValue
        {
            set
            {
                Q1.ParamValue = value;
                Q2.ParamValue = value;
                Q3.ParamValue = value;
                Q4.ParamValue = value;
            }
        }

        protected BinaryParam(BoolBaseParam q1, BoolBaseParam q2, BoolBaseParam q3, BoolBaseParam q4)
        {
            Q1 = q1;
            Q2 = q2;
            Q3 = q3;
            Q4 = q4;
        }

        protected void ResetParams()
        {
            Q1.ResetParam();
            Q2.ResetParam();
            Q3.ResetParam();
            Q4.ResetParam();
        }

        protected void ZeroParams()
        {
            Q1.ParamIndex = null;
            Q2.ParamIndex = null;
            Q3.ParamIndex = null;
            Q4.ParamIndex = null;
        }
    }
}

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