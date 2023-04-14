using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Params.Expressions.Legacy.Lip
{
    internal interface ICombinedShape
    {
        float GetBlendedLipShape(UnifiedTrackingData inputMap);
    }

    internal class PositiveNegativeShape : ICombinedShape
    {
        private readonly SRanipal_LipShape_v2 _positiveShape, _negativeShape;
        private float _positiveCache, _negativeCache;
        private bool _steps;

        internal PositiveNegativeShape(SRanipal_LipShape_v2 positiveShape, SRanipal_LipShape_v2 negativeShape, bool steps = false)
        {
            _positiveShape = positiveShape;
            _negativeShape = negativeShape;
            _steps = steps;
        }

        public float GetBlendedLipShape(UnifiedTrackingData inputMap)
        {
            _positiveCache = UnifiedSRanMapper.GetTransformedShape(_positiveShape, inputMap);
            _negativeCache = UnifiedSRanMapper.GetTransformedShape(_negativeShape, inputMap) * -1;
            return _steps ? (_positiveCache - _negativeCache) - 1 : _positiveCache + _negativeCache;
        }
    }

    internal class PositiveNegativeAveragedShape : ICombinedShape
    {
        private readonly SRanipal_LipShape_v2[] _positiveShapes, _negativeShapes;
        private readonly float[] _positiveCache, _negativeCache;
        private readonly int _positiveCount, _negativeCount;
        private readonly bool _useMax;

        internal PositiveNegativeAveragedShape(SRanipal_LipShape_v2[] positiveShapes, SRanipal_LipShape_v2[] negativeShapes)
        {
            _positiveShapes = positiveShapes;
            _negativeShapes = negativeShapes;
            _positiveCache = new float[positiveShapes.Length];
            _negativeCache = new float[negativeShapes.Length];
            _positiveCount = positiveShapes.Length;
            _negativeCount = negativeShapes.Length;
        }

        internal PositiveNegativeAveragedShape(SRanipal_LipShape_v2[] positiveShapes, SRanipal_LipShape_v2[] negativeShapes, bool useMax)
        {
            _positiveShapes = positiveShapes;
            _negativeShapes = negativeShapes;
            _positiveCache = new float[positiveShapes.Length];
            _negativeCache = new float[negativeShapes.Length];
            _positiveCount = positiveShapes.Length;
            _negativeCount = negativeShapes.Length;
            _useMax = useMax;
        }

        public float GetBlendedLipShape(UnifiedTrackingData inputMap)
        {                      
            if (!_useMax)
            {
                float positive = 0;
                float negative = 0;

                for (int i = 0; i < _positiveCount; i++) {
                        _positiveCache[i] = UnifiedSRanMapper.GetTransformedShape(_positiveShapes[i], inputMap);
                    positive += _positiveCache[i];
                }

                for (int i = 0; i < _negativeCount; i++) {
                        _negativeCache[i] = UnifiedSRanMapper.GetTransformedShape(_negativeShapes[i], inputMap) * -1.0f;
                    negative += _negativeCache[i];
                }

                return (positive / _positiveCount) + (negative / _negativeCount);
            }

            for (int i = 0; i < _positiveCount; i++) {
                     _positiveCache[i] = UnifiedSRanMapper.GetTransformedShape(_positiveShapes[i], inputMap);     
            }

            for (int i = 0; i < _negativeCount; i++) {
                    _negativeCache[i] = UnifiedSRanMapper.GetTransformedShape(_negativeShapes[i], inputMap);
            }

            return _positiveCache.Max() + (-1) * _negativeCache.Max();
        }
    }
}