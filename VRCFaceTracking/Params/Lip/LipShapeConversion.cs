using System;
using System.Linq;
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

    public class SpecialtyShape
    {
        private readonly LipShape_v2[] _shapes;
        private float[] _caches;

        public SpecialtyShape(LipShape_v2[] shapes)
        {
            _shapes = shapes;
            _caches = new float[shapes.Length];
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            //Assign Values to Cache
            for (byte i = 0; i < _shapes.Length; i++)
                if (inputMap.ContainsKey(_shapes[i]))
                    _caches[i] = inputMap[_shapes[i]];


            //Stacking for two paramaters. Only usefull for TongueSteps.
            if (_caches.Length == 2)
                return (_caches[0] + _caches[1]) - 1;


            //Compression for 4 in 1 params (-1 to -.5 , -.5 to 0 , etc)
            if (_caches.Length == 4)
                for (byte i = 0; i < 4; i++)
                    if (_caches[i] == _caches.Max())
                        return NormalizeFloat(
                            0,
                            1,
                            (i * (2f / _caches.Length)) + .01f,
                            ((i * (2f / _caches.Length)) + 2f / _caches.Length) - .01f,
                            _caches.Max()) - 1;
            return 0;
        }
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}