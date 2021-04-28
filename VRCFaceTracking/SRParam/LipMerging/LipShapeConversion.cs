using System;
using System.Collections.Generic;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRParam.LipMerging
{
    public class PositiveNegativeShape
    {
        public readonly LipShape_v2 PositiveShape, NegativeShape;
        private float _positiveCache, _negativeCache, _positiveMax, _negativeMax;
        
        public PositiveNegativeShape(LipShape_v2 positiveShape, LipShape_v2 negativeShape)
        {
            PositiveShape = positiveShape;
            NegativeShape = negativeShape;
        }

        public void ResetMinMaxRange()
        {
            _positiveMax = 0;
            _negativeMax = 0;
        }   // Reset on level change
        
        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            float currentPositive = _positiveCache, currentNegative = _negativeCache;   // Initialize currents with last known values in case SRanipal doesn't return them this frame
            if (inputMap.ContainsKey(PositiveShape)) currentPositive = inputMap[PositiveShape];
            if (inputMap.ContainsKey(NegativeShape)) currentNegative = inputMap[NegativeShape];

            // Make sure floats are not negative
            currentPositive = ClampFloat(currentPositive);
            currentNegative = ClampFloat(currentNegative);
            
            UpdateMinMaxThresholds(currentPositive, currentNegative);   // Update maximum recorded floats for positive and negative
            
            // Scale the floats to fix into their recorded maximum value, convert the output to floats, make the negative float actually negative and set caches
            _positiveCache = (float)ScaleFloat(currentPositive, _positiveMax);
            _negativeCache = (float)(ScaleFloat(currentNegative, _negativeMax) * -1);
            
            return _positiveCache + _negativeCache;
        }

        private static float ClampFloat(float input) => input < 0 ? 0 : input; // We know the float cannot be above +1 as we divide it by itself or higher later

        private void UpdateMinMaxThresholds(float positive, float negative)
        {
            if (positive > _positiveMax) _positiveMax = positive;
            if (negative > _negativeMax) _negativeMax = negative;
        }

        private static double ScaleFloat(float input, float recordedMax) => Math.Round(input / recordedMax, 2);
    }
}