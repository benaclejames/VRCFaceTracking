using System;
using System.Collections.Generic;
using ParamLib;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.Lip
{
    public class LipParameter : FloatBaseParam, IParameter
    {
        public LipParameter(string paramName, Func<Dictionary<LipShape_v2, float>, float[], float?> getValueFunc,
            bool prioritised = false)
            : base(paramName, prioritised)
        {
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                if (lip == null || floats == null) return;

                var newParamValue = getValueFunc.Invoke(floats, lip);
                if (newParamValue.HasValue) ParamValue = newParamValue.Value;
            };
        }

        public string[] GetName() => new [] {ParamName};
    }
}