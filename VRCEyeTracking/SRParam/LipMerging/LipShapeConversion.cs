using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCEyeTracking.SRParam.LipMerging
{
    public class PositiveNegativeShape
    {
        public readonly LipShape_v2 PositiveShape, NegativeShape;
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape)
        {
            PositiveShape = positiveShape;
            NegativeShape = negativeShape;
        }


        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            float positiveValue=0f, negativeValue=0f;
            if (inputMap.ContainsKey(PositiveShape)) positiveValue = inputMap[PositiveShape];
            if (inputMap.ContainsKey(NegativeShape)) negativeValue = inputMap[NegativeShape] * -1;
            return positiveValue + negativeValue;
        }
    }
}