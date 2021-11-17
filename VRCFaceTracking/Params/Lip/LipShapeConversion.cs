using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.Lip
{
    public class PositiveNegativeShape
    {
        private readonly LipShape_v2 _positiveShape, _negativeShape;
        private float _positiveCache, _negativeCache;
        
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape)
        {
            _positiveShape = positiveShape;
            _negativeShape = negativeShape;
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            if (inputMap.ContainsKey(_positiveShape)) _positiveCache = inputMap[_positiveShape];
            if (inputMap.ContainsKey(_negativeShape)) _negativeCache = inputMap[_negativeShape] * -1;
            return _positiveCache + _negativeCache;
        }
    }
}