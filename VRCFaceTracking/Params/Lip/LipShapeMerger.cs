using System;
using System.Collections.Generic;
using System.Linq;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params.Lip;

namespace VRCFaceTracking.Params.LipMerging
{
    public static class LipShapeMerger
    {
        private static readonly Dictionary<string, ICombinedShape> MergedShapes =
            new Dictionary<string, ICombinedShape>
            {
                {"JawX", new PositiveNegativeShape(LipShape_v3.JawRight, LipShape_v3.JawLeft)},
                {"MouthUpper", new PositiveNegativeShape(LipShape_v3.MouthUpperRight, LipShape_v3.MouthUpperLeft)},
                {"MouthLower", new PositiveNegativeShape(LipShape_v3.MouthLowerRight, LipShape_v3.MouthLowerLeft)},
                {"MouthX", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperRight, LipShape_v3.MouthLowerRight}, new LipShape_v3[]{LipShape_v3.MouthUpperLeft, LipShape_v3.MouthLowerLeft}, true)},
                {"SmileSadRight", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthSadRight)},
                {"SmileSadLeft", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSadLeft)},
                {"SmileSad", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthSadLeft, LipShape_v3.MouthSadRight})},
                {"TongueY", new PositiveNegativeShape(LipShape_v3.TongueUp, LipShape_v3.TongueDown)},
                {"TongueX", new PositiveNegativeShape(LipShape_v3.TongueRight, LipShape_v3.TongueLeft)},
                {"PuffSuckRight", new PositiveNegativeShape(LipShape_v3.CheekPuffRight, LipShape_v3.CheekSuck)},
                {"PuffSuckLeft", new PositiveNegativeShape(LipShape_v3.CheekPuffLeft, LipShape_v3.CheekSuck)},
                {"PuffSuck", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffLeft, LipShape_v3.CheekPuffRight}, new LipShape_v3[]{LipShape_v3.CheekSuck}, true)},

                //Additional combined shapes created with the help of the VRCFT Discord!

                //JawOpen based params
                {"JawOpenApe", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.MouthApeShape)},
                {"JawOpenPuff", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.JawOpen}, new LipShape_v3[]{LipShape_v3.CheekPuffLeft, LipShape_v3.CheekPuffRight})},
                {"JawOpenPuffRight", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.CheekPuffRight)},
                {"JawOpenPuffLeft", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.CheekPuffLeft)},
                {"JawOpenSuck", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.CheekSuck)},
                {"JawOpenForward", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.JawForward)},
                {"JawOpenOverlay", new PositiveNegativeShape(LipShape_v3.JawOpen, LipShape_v3.MouthLowerOverlay)},

                //MouthUpperUpRight based params
                {"MouthUpperUpRightUpperInside", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.MouthUpperInside)},
                {"MouthUpperUpRightPuffRight", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.CheekPuffRight)},
                {"MouthUpperUpRightApe", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.MouthApeShape)},
                {"MouthUpperUpRightPout", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.MouthPout)},
                {"MouthUpperUpRightOverlay", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.MouthLowerOverlay)},
                {"MouthUpperUpRightSuck", new PositiveNegativeShape(LipShape_v3.MouthUpperUpRight, LipShape_v3.CheekSuck)},
                
                //MouthUpperUpLeft based params
                {"MouthUpperUpLeftUpperInside", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperInside)},
                {"MouthUpperUpLeftPuffLeft", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.CheekPuffLeft)},
                {"MouthUpperUpLeftApe", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthApeShape)},
                {"MouthUpperUpLeftPout", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthPout)},
                {"MouthUpperUpLeftOverlay", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthLowerOverlay)},
                {"MouthUpperUpLeftSuck", new PositiveNegativeShape(LipShape_v3.MouthUpperUpLeft, LipShape_v3.CheekSuck)},

                // MouthUpperUp Left+Right base params
                {"MouthUpperUpUpperInside", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.MouthUpperInside })},
                {"MouthUpperUpInside", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.MouthUpperInside, LipShape_v3.MouthLowerInside}, true)},
                {"MouthUpperUpPuff", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.CheekPuffLeft, LipShape_v3.CheekPuffRight})},
                {"MouthUpperUpPuffLeft", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.CheekPuffLeft})},
                {"MouthUpperUpPuffRight", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.CheekPuffRight})},
                {"MouthUpperUpApe", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.MouthApeShape})},
                {"MouthUpperUpPout", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.MouthPout})},
                {"MouthUpperUpOverlay", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.MouthLowerOverlay})},
                {"MouthUpperUpSuck", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthUpperUpLeft, LipShape_v3.MouthUpperUpRight}, new LipShape_v3[]{LipShape_v3.CheekSuck})},

                //MouthLowerDownRight based params
                {"MouthLowerDownRightLowerInside", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.MouthLowerInside)},
                {"MouthLowerDownRightPuffRight", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.CheekPuffRight)},
                {"MouthLowerDownRightApe", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.MouthApeShape)},
                {"MouthLowerDownRightPout", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.MouthPout)},
                {"MouthLowerDownRightOverlay", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.MouthLowerOverlay)},
                {"MouthLowerDownRightSuck", new PositiveNegativeShape(LipShape_v3.MouthLowerDownRight, LipShape_v3.CheekSuck)},

                //MouthLowerDownLeft based params
                {"MouthLowerDownLeftLowerInside", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerInside)},
                {"MouthLowerDownLeftPuffLeft", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.CheekPuffLeft)},
                {"MouthLowerDownLeftApe", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthApeShape)},
                {"MouthLowerDownLeftPout", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthPout)},
                {"MouthLowerDownLeftOverlay", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerOverlay)},
                {"MouthLowerDownLeftSuck", new PositiveNegativeShape(LipShape_v3.MouthLowerDownLeft, LipShape_v3.CheekSuck)},

                // MouthLowerDown Left+Right base params
                {"MouthLowerDownLowerInside", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.MouthLowerInside})},
                {"MouthLowerDownInside", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.MouthUpperInside, LipShape_v3.MouthLowerInside}, true)},
                {"MouthLowerDownPuff", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.CheekPuffLeft, LipShape_v3.CheekPuffRight})},
                {"MouthLowerDownPuffLeft", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.CheekPuffLeft})},
                {"MouthLowerDownPuffRight", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.CheekPuffRight})},
                {"MouthLowerDownApe", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.MouthApeShape})},
                {"MouthLowerDownPout", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.MouthPout})},
                {"MouthLowerDownOverlay", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.MouthLowerOverlay})},
                {"MouthLowerDownSuck", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthLowerDownLeft, LipShape_v3.MouthLowerDownRight}, new LipShape_v3[]{LipShape_v3.CheekSuck})},

				// MouthInsideOverturn based params
				{"MouthUpperInsideOverturn", new PositiveNegativeShape(LipShape_v3.MouthUpperInside, LipShape_v3.MouthUpperOverturn)},
				{"MouthLowerInsideOverturn", new PositiveNegativeShape(LipShape_v3.MouthLowerInside, LipShape_v3.MouthLowerOverturn)},
				
                //SmileRight based params; Recommend using these if you already have SmileSadLeft setup!
                {"SmileRightUpperOverturn", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthUpperOverturn)},
                {"SmileRightLowerOverturn", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthLowerOverturn)},
                {"SmileRightOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn})},
                {"SmileRightApe", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthApeShape)},
                {"SmileRightOverlay", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthLowerOverlay)},
                {"SmileRightPout", new PositiveNegativeShape(LipShape_v3.MouthSmileRight, LipShape_v3.MouthPout)},

                //SmileLeft based params; Recommend using these if you already have SmileSadRight setup!
                {"SmileLeftUpperOverturn", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthUpperOverturn)},
                {"SmileLeftLowerOverturn", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthLowerOverturn)},
                {"SmileLeftOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn})},
                {"SmileLeftApe", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthApeShape)},
                {"SmileLeftOverlay", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthLowerOverlay)},
                {"SmileLeftPout", new PositiveNegativeShape(LipShape_v3.MouthSmileLeft, LipShape_v3.MouthPout)},

                //Smile Left+Right based params
                {"SmileUpperOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn})},
                {"SmileLowerOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthLowerOverturn})},
                {"SmileOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn})},
                {"SmileApe", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthApeShape})},
                {"SmileOverlay", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthLowerOverlay})},
                {"SmilePout", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.MouthSmileLeft, LipShape_v3.MouthSmileRight}, new LipShape_v3[]{LipShape_v3.MouthPout})},

                //CheekPuffRight based params
                {"PuffRightUpperOverturn", new PositiveNegativeShape(LipShape_v3.CheekPuffRight, LipShape_v3.MouthUpperOverturn)},
                {"PuffRightLowerOverturn", new PositiveNegativeShape(LipShape_v3.CheekPuffRight, LipShape_v3.MouthLowerOverturn)},
                {"PuffRightOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffRight}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn}, true)},

                //CheekPuffLeft based params
                {"PuffLeftUpperOverturn", new PositiveNegativeShape(LipShape_v3.CheekPuffLeft, LipShape_v3.MouthUpperOverturn)},
                {"PuffLeftLowerOverturn", new PositiveNegativeShape(LipShape_v3.CheekPuffLeft, LipShape_v3.MouthLowerOverturn)},
                {"PuffLeftOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffLeft}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn}, true)},

                //CheekPuff Left+Right based params
                {"PuffUpperOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffRight, LipShape_v3.CheekPuffLeft}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn})},
                {"PuffLowerOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffRight, LipShape_v3.CheekPuffLeft}, new LipShape_v3[]{LipShape_v3.MouthLowerOverturn})},
                {"PuffOverturn", new PositiveNegativeAveragedShape(new LipShape_v3[]{LipShape_v3.CheekPuffRight, LipShape_v3.CheekPuffLeft}, new LipShape_v3[]{LipShape_v3.MouthUpperOverturn, LipShape_v3.MouthLowerOverturn}, true)},

                //Combine both TongueSteps (-1 fully in, 0 on edge, 1 fully out)
                {"TongueSteps", new PositiveNegativeShape(LipShape_v3.TongueLongStep1, LipShape_v3.TongueLongStep2, true)},
            };
        
        // Make a list called LipParameters containing the results from both GetOptimizedLipParameters and GetAllLipParameters, and add GetLipActivatedStatus
        public static readonly IParameter[] AllLipParameters =
            GetAllLipShapes().Union(GetOptimizedLipParameters()).Union(GetLipActivatedStatus()).ToArray();

        public static bool IsLipShapeName(string name) => MergedShapes.ContainsKey(name) || Enum.TryParse(name, out LipShape_v3 shape);
        
        private static IEnumerable<EParam> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new EParam((eye, lip) => 
                shape.Value.GetBlendedLipShape(lip.LatestShapes), shape.Key, 0.0f));

        private static IEnumerable<EParam> GetAllLipShapes() =>
            ((LipShape_v3[]) Enum.GetValues(typeof(LipShape_v3))).ToList().Select(shape =>
                new EParam((eye, lip) => lip.LatestShapes[(int)shape],
                    shape.ToString(), 0.0f));

        private static IEnumerable<IParameter> GetLipActivatedStatus() => new List<IParameter>
        {
            new BoolParameter(v2 => UnifiedLibManager.LipStatus.Equals(ModuleState.Active), "LipTrackingActive"),
        };
    }
}