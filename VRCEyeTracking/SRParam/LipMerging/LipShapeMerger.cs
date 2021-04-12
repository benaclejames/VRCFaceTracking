using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;

namespace VRCEyeTracking.SRParam.LipMerging
{
    public static class LipShapeMerger
    {
        private enum OptimizedLipShape
        {
            Jaw_X,
            Mouth_Upper,
            Mouth_Lower,
            Smile_Sad_Right,
            Smile_Sad_Left,
            Tongue_Y,
            Tongue_X
        }

        private static readonly Dictionary<OptimizedLipShape, PositiveNegativeShape> OptimizedLipShapes =
            new Dictionary<OptimizedLipShape, PositiveNegativeShape>
            {
                {OptimizedLipShape.Jaw_X, new PositiveNegativeShape(LipShape_v2.Jaw_Right, LipShape_v2.Jaw_Left)},
                {OptimizedLipShape.Mouth_Upper, new PositiveNegativeShape(LipShape_v2.Mouth_Upper_Right, LipShape_v2.Mouth_Upper_Left)},
                {OptimizedLipShape.Mouth_Lower, new PositiveNegativeShape(LipShape_v2.Mouth_Lower_Right, LipShape_v2.Mouth_Lower_Left)},
                {OptimizedLipShape.Smile_Sad_Right, new PositiveNegativeShape(LipShape_v2.Mouth_Smile_Right, LipShape_v2.Mouth_Sad_Right)},
                {OptimizedLipShape.Smile_Sad_Left, new PositiveNegativeShape(LipShape_v2.Mouth_Smile_Left, LipShape_v2.Mouth_Sad_Left)},
                {OptimizedLipShape.Tongue_Y, new PositiveNegativeShape(LipShape_v2.Tongue_Up, LipShape_v2.Tongue_Down)},
                {OptimizedLipShape.Tongue_X, new PositiveNegativeShape(LipShape_v2.Tongue_Right, LipShape_v2.Tongue_Left)},
            };

        public static IEnumerable<SRanipalLipParameter> GetOptimizedLipParameters() => OptimizedLipShapes
            .Select(optimizedShape => new SRanipalLipParameter(v2 
                => optimizedShape.Value.GetBlendedLipShape(v2)
            , optimizedShape.Key.ToString(), true));

        public static IEnumerable<LipShape_v2> GetUnoptimizedLipShapes()
        {
            var unoptimizedShapes = ((LipShape_v2[]) Enum.GetValues(typeof(LipShape_v2))).ToList();
            foreach (var optimization in OptimizedLipShapes)
            {
                unoptimizedShapes.Remove(optimization.Value.PositiveShape);
                unoptimizedShapes.Remove(optimization.Value.NegativeShape);
            }
            return unoptimizedShapes;
        }
        
        public static void ResetLipShapeMinMaxThresholds()
        {
            foreach (var positiveNegativeShape in OptimizedLipShapes)
            {
                positiveNegativeShape.Value.ResetMinMaxRange();
            }
        }
    }
}