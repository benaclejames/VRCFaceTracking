using System;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using ParamLib;

namespace VRCFaceTracking.SRParam
{
    public class SRanipalFloatEyeParameter : FloatBaseParam, ISRanipalParam
    {
        private readonly Func<EyeData_v2, float> _getSRanipalParam;
        
        public SRanipalFloatEyeParameter(Func<EyeData_v2, float> getValueFunc, string paramName, bool prioritised = false) 
            : base(paramName, prioritised) => _getSRanipalParam = getValueFunc;
        
        public void RefreshParam(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (eyeData == null) return;
            ParamValue = _getSRanipalParam.Invoke(eyeData.Value);
        }
    }

    public class SRanipalXYEyeParameter : XYParam, ISRanipalParam
    {
        private readonly Func<EyeData_v2, Vector2?> _getSRanipalParam;  // Can be null cus blinking messes with eye look validity

        public SRanipalXYEyeParameter(Func<EyeData_v2, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
            => _getSRanipalParam = getValueFunc;

        public void RefreshParam(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (eyeData == null) return;
            var newValue = _getSRanipalParam.Invoke(eyeData.Value);
            if (newValue.HasValue) ParamValue = newValue.Value;
        }

        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
    }

    public class SRanipalBoolEyeParameter : BoolBaseParam, ISRanipalParam
    {
        private readonly Func<EyeData_v2, bool> _getSRanipalParam;

        public SRanipalBoolEyeParameter(Func<EyeData_v2, bool> getValueFunc, string paramName) : base(paramName) =>
            _getSRanipalParam = getValueFunc;

        public void RefreshParam(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (eyeData == null) return;
            ParamValue = _getSRanipalParam.Invoke(eyeData.Value);
        }
    }
}