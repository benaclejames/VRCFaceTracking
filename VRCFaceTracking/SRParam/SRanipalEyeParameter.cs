using System;
using UnityEngine;
using ViveSR.anipal.Eye;
using ParamLib;

namespace VRCFaceTracking.SRParam
{
    public class SRanipalFloatEyeParameter : FloatBaseParam, ISRanipalParam
    {
        public SRanipalFloatEyeParameter(Func<EyeData_v2, float> getValueFunc, string paramName, bool prioritised = false) 
            : base(paramName, prioritised)
        {
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) =>
            {
                if (!eye.HasValue) return;
                ParamValue = getValueFunc.Invoke(eye.Value);
            };
        }
    }

    public class SRanipalXYEyeParameter : XYParam, ISRanipalParam
    {
        public SRanipalXYEyeParameter(Func<EyeData_v2, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) =>
            {
                if (!eye.HasValue) return;
                var newValue = getValueFunc.Invoke(eye.Value);
                if (newValue.HasValue) ParamValue = newValue.Value;
            };
        }

        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
    }

    public class SRanipalBoolEyeParameter : BoolBaseParam, ISRanipalParam
    {
        public SRanipalBoolEyeParameter(Func<EyeData_v2, bool> getValueFunc, string paramName) : base(paramName)
        {
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) =>
            {
                if (!eye.HasValue) return;
                ParamValue = getValueFunc.Invoke(eye.Value);
            };
        }
    }
}