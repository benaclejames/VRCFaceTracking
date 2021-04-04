using System;
using System.Collections.Generic;
using MelonLoader;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.ParamLib;

namespace VRCEyeTracking.SRParam
{
    public class SRanipalLipParameter : FloatBaseParam, ISRanipalParam
    {
        private readonly Func<Dictionary<LipShape_v2, float>, float?> _getSRanipalParam;

        public SRanipalLipParameter(Func<Dictionary<LipShape_v2, float>, float?> getValueFunc, string paramName,
            bool prioritised = false)
            : base(paramName, prioritised) => _getSRanipalParam = getValueFunc;

        public void RefreshParam(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (lipData == null) return;
            var newParamValue = _getSRanipalParam.Invoke(lipData);
            if (newParamValue.HasValue) ParamValue = newParamValue.Value;
        }

        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ParamIndex = null;
    }
}