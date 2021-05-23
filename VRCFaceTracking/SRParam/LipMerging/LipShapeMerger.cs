using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRParam.LipMerging
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

        public static readonly List<SRanipalLipParameter> VisemeShapes = new List<SRanipalLipParameter>
        {
            new SRanipalLipParameter("TongueRoll", (floats, lip) => lip[31], true),
            new SRanipalLipParameter("LipUpperLeftRaise", (floats, lip) => lip[20], true),
            new SRanipalLipParameter("LipUpperRightRaise", (floats, lip) => lip[19], true),
            new SRanipalLipParameter("LipLowerLeftRaise", (floats, lip) => lip[22], true),
            new SRanipalLipParameter("LipLowerRightRaise", (floats, lip) => lip[21], true),
            new SRanipalLipParameter("LipUpperHorizontal", (floats, lip) => lip[5] - lip[6], true),
            new SRanipalLipParameter("LipLowerHorizontal", (floats, lip) => lip[7] - lip[8], true),
            new SRanipalLipParameter("MouthLeftSmileFrown", (floats, lip) => lip[13] - lip[15], true),
            new SRanipalLipParameter("MouthRightSmileFrown", (floats, lip) => lip[12] - lip[14], true),
            new SRanipalLipParameter("MouthPout", (floats, lip) => lip[11], true),
            new SRanipalLipParameter("LipTopOverturn", (floats, lip) => lip[9], true),
            new SRanipalLipParameter("LipBottomOverturn", (floats, lip) => lip[10], true),
            new SRanipalLipParameter("LipTopOverUnder", (floats, lip) => 0 - lip[23], true),
            new SRanipalLipParameter("LipBottomOverUnder", (floats, lip) => lip[25] - lip[24], true),
            new SRanipalLipParameter("CheekLeftPuffSuck", (floats, lip) => lip[17] - lip[18], true),
            new SRanipalLipParameter("CheekRightPuffSuck", (floats, lip) => lip[16] - lip[18], true),
        };

        public static IEnumerable<SRanipalLipParameter> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new SRanipalLipParameter(shape.Key, (eye, lip) => 
                shape.Value.GetBlendedLipShape(eye), true)).ToList();

        public static IEnumerable<LipShape_v2> GetAllLipShapes() => ((LipShape_v2[]) Enum.GetValues(typeof(LipShape_v2))).ToList();
        
        public static void ResetLipShapeMinMaxThresholds()
        {
            foreach (var positiveNegativeShape in MergedShapes)
                positiveNegativeShape.Value.ResetMinMaxRange();
        }
    }
}