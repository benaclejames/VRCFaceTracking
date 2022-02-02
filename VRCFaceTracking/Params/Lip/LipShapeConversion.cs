using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.Lip
{

    public interface ICombinedShape
    {
        float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap);
    }

    public class PositiveNegativeShape : ICombinedShape
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
            if (inputMap.TryGetValue(_positiveShape, out var positiveResult)) _positiveCache = positiveResult;
            if (inputMap.TryGetValue(_negativeShape, out var negativeResult)) _negativeCache = negativeResult * -1;
            return _positiveCache + _negativeCache;
        }
    }

    public class PositiveNegativeAveragedShape : ICombinedShape
    {
        private readonly LipShape_v2[] _positiveShapes, _negativeShapes;
        private readonly float[] _positiveCache, _negativeCache;
        private readonly int _positiveCount, _negativeCount;

        public PositiveNegativeAveragedShape(LipShape_v2[] positiveShapes, LipShape_v2[] negativeShapes)
        {
            _positiveShapes = positiveShapes;
            _negativeShapes = negativeShapes;
            _positiveCache = new float[positiveShapes.Length];
            _negativeCache = new float[negativeShapes.Length];
            _positiveCount = positiveShapes.Length;
            _negativeCount = negativeShapes.Length;
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            float positive = 0;
            float negative = 0;
            for (int i = 0; i < _positiveCount; i++) {
                if (inputMap.TryGetValue(_positiveShapes[i], out var positiveResult))
                    _positiveCache[i] = positiveResult;
                positive += _positiveCache[i];
            }

            for (int i = 0; i < _negativeCount; i++) {
                if (inputMap.TryGetValue(_negativeShapes[i], out var negativeResult))
                    _negativeCache[i] = negativeResult * -1;
                negative += _negativeCache[i];
            }

            return (positive / _positiveCount) + (negative / _negativeCount);
        }
    }

}