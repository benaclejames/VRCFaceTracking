using System.Collections.Generic;
using ViveSR.anipal.Lip;
using System.Linq;

namespace VRCFaceTracking.Params.Lip
{
    public interface ICombinedShape
    {
        float GetBlendedLipShape(float[] inputMap);
    }

    public class PositiveNegativeShape : ICombinedShape
    {
        private readonly int _positiveShape, _negativeShape;
        private float _positiveCache, _negativeCache;
        private bool _steps;
        
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape, bool steps = false)
        {
            _positiveShape = (int)positiveShape;
            _negativeShape = (int)negativeShape;
            _steps = steps;
        }

        public float GetBlendedLipShape(float[] inputMap)
        {
            _positiveCache = inputMap[_positiveShape];
            _negativeCache = inputMap[_negativeShape] * -1;
            return _steps ? (_positiveCache - _negativeCache) - 1 : _positiveCache + _negativeCache;
        }
    }

    public class PositiveNegativeAveragedShape : ICombinedShape
    {
        private readonly int[] _positiveShapes, _negativeShapes;
        private readonly float[] _positiveCache, _negativeCache;
        private readonly int _positiveCount, _negativeCount;
        private readonly bool _useMax;

        public PositiveNegativeAveragedShape(LipShape_v2[] positiveShapes, LipShape_v2[] negativeShapes)
        {
            _positiveShapes = positiveShapes.Select(s => (int)s).ToArray();
            _negativeShapes = negativeShapes.Select(s => (int)s).ToArray();
            _positiveCache = new float[positiveShapes.Length];
            _negativeCache = new float[negativeShapes.Length];
            _positiveCount = positiveShapes.Length;
            _negativeCount = negativeShapes.Length;
        }

        public PositiveNegativeAveragedShape(LipShape_v2[] positiveShapes, LipShape_v2[] negativeShapes, bool useMax)
        {
            _positiveShapes = positiveShapes.Select(s => (int)s).ToArray();
            _negativeShapes = negativeShapes.Select(s => (int)s).ToArray();
            _positiveCache = new float[positiveShapes.Length];
            _negativeCache = new float[negativeShapes.Length];
            _positiveCount = positiveShapes.Length;
            _negativeCount = negativeShapes.Length;
            _useMax = useMax;
        }

        public float GetBlendedLipShape(float[] inputMap)
        {                      
            if (!_useMax)
            {
                float positive = 0;
                float negative = 0;

                for (int i = 0; i < _positiveCount; i++) {
                        _positiveCache[i] = inputMap[_positiveShapes[i]];
                    positive += _positiveCache[i];
                }

                for (int i = 0; i < _negativeCount; i++) {
                        _negativeCache[i] = inputMap[_negativeShapes[i]] * -1;
                    negative += _negativeCache[i];
                }

                return (positive / _positiveCount) + (negative / _negativeCount);
            }

            for (int i = 0; i < _positiveCount; i++) {
                     _positiveCache[i] = inputMap[_positiveShapes[i]];     
            }

            for (int i = 0; i < _negativeCount; i++) {
                    _negativeCache[i] = inputMap[_negativeShapes[i]];
            }

            return _positiveCache.Max() + (-1) * _negativeCache.Max();
        }
    }
}