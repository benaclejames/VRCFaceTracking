using System;
using System.Collections.Generic;
using ViveSR.anipal.Lip;
using ParamLib;

namespace VRCFaceTracking.SRParam
{
    public class SRanipalLipParameter : FloatBaseParam, ISRanipalParam
    {
        public SRanipalLipParameter(string paramName, Func<Dictionary<LipShape_v2, float>, float[], float?> getValueFunc,
            bool prioritised = false)
            : base(paramName, prioritised)
        {
            MainMod.OnSRanipalParamsUpdated += (eye, lip, floats) =>
            {
                if (lip == null) return;

                var newParamValue = getValueFunc.Invoke(floats, lip);
                if (newParamValue.HasValue) ParamValue = newParamValue.Value;
            };
        }

        void ISRanipalParam.ResetParam() => ResetParam();
    }
}