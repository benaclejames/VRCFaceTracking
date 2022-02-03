using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.Lip
{
    public class PositiveNegativeShape
    {
        private readonly LipShape_v2 _positiveShape, _negativeShape;
        private float _positiveCache, _negativeCache;
        private bool _steps;
        
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape, bool steps = false)
        {
            _positiveShape = positiveShape;
            _negativeShape = negativeShape;
            _steps = steps;
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            if (inputMap.TryGetValue(_positiveShape, out var positiveResult)) _positiveCache = positiveResult;
            if (inputMap.TryGetValue(_negativeShape, out var negativeResult)) _negativeCache = negativeResult * -1;
            return _steps ? (_positiveCache - _negativeCache) - 1 : _positiveCache + _negativeCache;
        }
    }
}
