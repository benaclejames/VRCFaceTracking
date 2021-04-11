using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCEyeTracking.SRParam.LipMerging
{
    public class PositiveNegativeShape
    {
        public readonly LipShape_v2 PositiveShape, NegativeShape;
        private float _positiveCache, _negativeCache;
        
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape)
        {
            PositiveShape = positiveShape;
            NegativeShape = negativeShape;
        }
        
        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            if (inputMap.ContainsKey(PositiveShape)) _positiveCache = inputMap[PositiveShape];
            if (inputMap.ContainsKey(NegativeShape)) _negativeCache = inputMap[NegativeShape]*-1;

            return _positiveCache + _negativeCache;
        }
    }
}