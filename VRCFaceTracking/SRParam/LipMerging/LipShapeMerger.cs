using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRParam.LipMerging
{
    public static class LipShapeMerger
    {
        private enum OptimizedLipShape
        {
            JawX,
            MouthUpper,
            MouthLower,
            SmileSadRight,
            SmileSadLeft,
            TongueY,
            TongueX,
            PuffSuckRight,
            PuffSuckLeft
        }

        private static readonly Dictionary<OptimizedLipShape, PositiveNegativeShape> OptimizedLipShapes =
            new Dictionary<OptimizedLipShape, PositiveNegativeShape>
            {
                {OptimizedLipShape.JawX, new PositiveNegativeShape(LipShape_v2.JawRight, LipShape_v2.JawLeft)},
                {OptimizedLipShape.MouthUpper, new PositiveNegativeShape(LipShape_v2.MouthUpperRight, LipShape_v2.MouthUpperLeft)},
                {OptimizedLipShape.MouthLower, new PositiveNegativeShape(LipShape_v2.MouthLowerRight, LipShape_v2.MouthLowerLeft)},
                {OptimizedLipShape.SmileSadRight, new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthSadRight)},
                {OptimizedLipShape.SmileSadLeft, new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthSadLeft)},
                {OptimizedLipShape.TongueY, new PositiveNegativeShape(LipShape_v2.TongueUp, LipShape_v2.TongueDown)},
                {OptimizedLipShape.TongueX, new PositiveNegativeShape(LipShape_v2.TongueRight, LipShape_v2.TongueLeft)},
                {OptimizedLipShape.PuffSuckRight, new PositiveNegativeShape(LipShape_v2.CheekPuffRight, LipShape_v2.CheekSuck)},
                {OptimizedLipShape.PuffSuckLeft, new PositiveNegativeShape(LipShape_v2.CheekPuffLeft, LipShape_v2.CheekSuck)},
            };

        public static IEnumerable<SRanipalLipParameter> GetOptimizedLipParameters() => OptimizedLipShapes
            .Select(optimizedShape => new SRanipalLipParameter(v2 
                => optimizedShape.Value.GetBlendedLipShape(v2)
            , optimizedShape.Key.ToString(), true));

        public static IEnumerable<LipShape_v2> GetUnoptimizedLipShapes()
        {
            return ((LipShape_v2[]) Enum.GetValues(typeof(LipShape_v2))).ToList();
            /*foreach (var optimization in OptimizedLipShapes)
            {
                unoptimizedShapes.Remove(optimization.Value.PositiveShape);
                unoptimizedShapes.Remove(optimization.Value.NegativeShape);
            }*/
            //return unoptimizedShapes;
        }
        
        public static void ResetLipShapeMinMaxThresholds()
        {
            foreach (var positiveNegativeShape in OptimizedLipShapes)
                positiveNegativeShape.Value.ResetMinMaxRange();
        }
    }
}