using System;
using MelonLoader;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.ParamLib;

namespace VRCEyeTracking.SRParam
{
    public class SRanipalLipParameter : FloatBaseParam, ISRanipalParam
    {
        private readonly Func<LipData_v2, float> _getSRanipalParam;

        public SRanipalLipParameter(Func<LipData_v2, float> getValueFunc, string paramName,
            bool prioritised = false)
            : base(paramName, prioritised) => _getSRanipalParam = getValueFunc;

        public void RefreshParam(EyeData_v2? eyeData, LipData_v2? lipData)
        {
            if (lipData?.prediction_data.blend_shape_weight == null) return;
            ParamValue = _getSRanipalParam.Invoke(lipData.Value);
        }

        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ParamIndex = null;
    }
}