using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.LipMerging
{
    public static class LipShapeMerger
    {
        private static readonly Dictionary<string, PositiveNegativeShape> MergedShapes =
            new Dictionary<string, PositiveNegativeShape>
            {
                {"JawX", new PositiveNegativeShape(LipShape_v2.JawRight, LipShape_v2.JawLeft)},
                {"MouthUpper", new PositiveNegativeShape(LipShape_v2.MouthUpperRight, LipShape_v2.MouthUpperLeft)},
                {"MouthLower", new PositiveNegativeShape(LipShape_v2.MouthLowerRight, LipShape_v2.MouthLowerLeft)},
                {"SmileSadRight", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthSadRight)},
                {"SmileSadLeft", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthSadLeft)},
                {"TongueY", new PositiveNegativeShape(LipShape_v2.TongueUp, LipShape_v2.TongueDown)},
                {"TongueX", new PositiveNegativeShape(LipShape_v2.TongueRight, LipShape_v2.TongueLeft)},
                {"PuffSuckRight", new PositiveNegativeShape(LipShape_v2.CheekPuffRight, LipShape_v2.CheekSuck)},
                {"PuffSuckLeft", new PositiveNegativeShape(LipShape_v2.CheekPuffLeft, LipShape_v2.CheekSuck)},
            };
        
        // Make a list called LipParameters containing the results from both GetOptimizedLipParameters and GetAllLipParameters
        public static List<LipParameter> AllLipParameters =
            new List<LipParameter>(GetAllLipShapes().Union(GetOptimizedLipParameters()));

        public static bool IsLipShapeName(string name) => MergedShapes.ContainsKey(name) || Enum.TryParse(name, out LipShape_v2 shape);
        
        private static IEnumerable<LipParameter> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new LipParameter(shape.Key, (eye, lip) => 
                shape.Value.GetBlendedLipShape(eye), true)).ToList();

        private static IEnumerable<LipParameter> GetAllLipShapes() => ((LipShape_v2[]) Enum.GetValues(typeof(LipShape_v2))).ToList().Select(shape => 
            new LipParameter(shape.ToString(), (eye, lip) =>
            {
                if (eye.TryGetValue(shape, out var retValue)) return retValue;
                return null;
            }, true));
    }
}