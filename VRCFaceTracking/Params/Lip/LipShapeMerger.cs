using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params.Lip;

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

                //Additional combined shapes created with the help of the VRCFT Discord!

                //JawOpen based params
                {"JawOpenApe", new PositiveNegativeShape(LipShape_v2.JawOpen, LipShape_v2.MouthApeShape)},
                {"JawOpenPuffRight", new PositiveNegativeShape(LipShape_v2.JawOpen, LipShape_v2.CheekPuffRight)},
                {"JawOpenPuffLeft", new PositiveNegativeShape(LipShape_v2.JawOpen, LipShape_v2.CheekPuffLeft)},
                {"JawOpenSuck", new PositiveNegativeShape(LipShape_v2.JawOpen, LipShape_v2.CheekSuck)},
                {"JawOpenForward", new PositiveNegativeShape(LipShape_v2.JawOpen, LipShape_v2.JawForward)},

                //MouthUpperUpRight based params
                {"MouthUpperUpRightUpperInside", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthUpperInside)},
                {"MouthUpperUpRightPuffRight", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.CheekPuffRight)},
                {"MouthUpperUpRightApe", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthApeShape)},
                {"MouthUpperUpRightPout", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthPout)},
                {"MouthUpperUpRightOverlay", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthLowerOverlay)},
                
                //MouthUpperUpLeft based params
                {"MouthUpperUpLeftUpperInside", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthUpperInside)},
                {"MouthUpperUpLeftPuffLeft", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.CheekPuffRight)},
                {"MouthUpperUpLeftApe", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthApeShape)},
                {"MouthUpperUpLeftPout", new PositiveNegativeShape(LipShape_v2.MouthUpperUpRight, LipShape_v2.MouthPout)},
                {"MouthUpperUpLeftOverlay", new PositiveNegativeShape(LipShape_v2.MouthUpperUpLeft, LipShape_v2.MouthLowerOverlay)},

                //MouthLowerDownRight based params
                {"MouthLowerDownRightLowerInside", new PositiveNegativeShape(LipShape_v2.MouthLowerDownRight, LipShape_v2.MouthLowerInside)},
                {"MouthLowerDownRightPuffRight", new PositiveNegativeShape(LipShape_v2.MouthLowerDownRight, LipShape_v2.CheekPuffRight)},
                {"MouthLowerDownRightApe", new PositiveNegativeShape(LipShape_v2.MouthLowerDownRight, LipShape_v2.MouthApeShape)},
                {"MouthLowerDownRightPout", new PositiveNegativeShape(LipShape_v2.MouthLowerDownRight, LipShape_v2.MouthPout)},
                {"MouthLowerDownRightOverlay", new PositiveNegativeShape(LipShape_v2.MouthLowerDownRight, LipShape_v2.MouthLowerOverlay)},

                //MouthLowerDownLeft based params
                {"MouthLowerDownLeftLowerInside", new PositiveNegativeShape(LipShape_v2.MouthLowerDownLeft, LipShape_v2.MouthLowerInside)},
                {"MouthLowerDownLeftPuffLeft", new PositiveNegativeShape(LipShape_v2.MouthLowerDownLeft, LipShape_v2.CheekPuffRight)},
                {"MouthLowerDownLeftApe", new PositiveNegativeShape(LipShape_v2.MouthLowerDownLeft, LipShape_v2.MouthApeShape)},
                {"MouthLowerDownLeftPout", new PositiveNegativeShape(LipShape_v2.MouthLowerDownLeft, LipShape_v2.MouthPout)},
                {"MouthLowerDownLeftOverlay", new PositiveNegativeShape(LipShape_v2.MouthLowerDownLeft, LipShape_v2.MouthLowerOverlay)},

                //SmileRight based params; Recommend using these if you already have SmileSadLeft setup!
                {"SmileRightUpperOverturn", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthUpperOverturn)},
                {"SmileRightLowerOverturn", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthLowerOverturn)},
                {"SmileRightApe", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthApeShape)},
                {"SmileRightOverlay", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthLowerOverlay)},
                {"SmileRightPout", new PositiveNegativeShape(LipShape_v2.MouthSmileRight, LipShape_v2.MouthPout)},

                //SmileLeft based params; Recommend using these if you already have SmileSadRight setup!
                {"SmileLeftUpperOverturn", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthUpperOverturn)},
                {"SmileLeftLowerOverturn", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthLowerOverturn)},
                {"SmileLeftApe", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthApeShape)},
                {"SmileLeftOverlay", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthLowerOverlay)},
                {"SmileLeftPout", new PositiveNegativeShape(LipShape_v2.MouthSmileLeft, LipShape_v2.MouthPout)},
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