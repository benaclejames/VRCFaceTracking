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
                {"JawX", new PositiveNegativeShape(UnifiedExpression.JawRight, UnifiedExpression.JawLeft)},
                {"MouthUpper", new PositiveNegativeShape(UnifiedExpression.MouthUpperRight, UnifiedExpression.MouthUpperLeft)},
                {"MouthLower", new PositiveNegativeShape(UnifiedExpression.MouthLowerRight, UnifiedExpression.MouthLowerLeft)},
                {"MouthX", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperRight, UnifiedExpression.MouthLowerRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperLeft, UnifiedExpression.MouthLowerLeft}, true)},
                {"SmileSadRight", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthSadRight)},
                {"SmileSadLeft", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSadLeft)},
                {"SmileSad", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthSadLeft, UnifiedExpression.MouthSadRight})},
                {"TongueY", new PositiveNegativeShape(UnifiedExpression.TongueUp, UnifiedExpression.TongueDown)},
                {"TongueX", new PositiveNegativeShape(UnifiedExpression.TongueRight, UnifiedExpression.TongueLeft)},
                {"PuffSuckRight", new PositiveNegativeShape(UnifiedExpression.CheekPuffRight, UnifiedExpression.CheekSuck)},
                {"PuffSuckLeft", new PositiveNegativeShape(UnifiedExpression.CheekPuffLeft, UnifiedExpression.CheekSuck)},
                {"PuffSuck", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft, UnifiedExpression.CheekPuffRight}, new UnifiedExpression[]{UnifiedExpression.CheekSuck}, true)},

                //Additional combined shapes created with the help of the VRCFT Discord!

                //JawOpen based params
                {"JawOpenApe", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.MouthApeShape)},
                {"JawOpenPuff", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.JawOpen}, new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft, UnifiedExpression.CheekPuffRight})},
                {"JawOpenPuffRight", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.CheekPuffRight)},
                {"JawOpenPuffLeft", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.CheekPuffLeft)},
                {"JawOpenSuck", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.CheekSuck)},
                {"JawOpenForward", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.JawForward)},
                {"JawOpenOverlay", new PositiveNegativeShape(UnifiedExpression.JawOpen, UnifiedExpression.MouthLowerOverlay)},

                //MouthUpperUpRight based params
                {"MouthUpperUpRightUpperInside", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.MouthUpperInside)},
                {"MouthUpperUpRightPuffRight", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.CheekPuffRight)},
                {"MouthUpperUpRightApe", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.MouthApeShape)},
                {"MouthUpperUpRightPout", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.MouthPout)},
                {"MouthUpperUpRightOverlay", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.MouthLowerOverlay)},
                {"MouthUpperUpRightSuck", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpRight, UnifiedExpression.CheekSuck)},
                
                //MouthUpperUpLeft based params
                {"MouthUpperUpLeftUpperInside", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperInside)},
                {"MouthUpperUpLeftPuffLeft", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.CheekPuffLeft)},
                {"MouthUpperUpLeftApe", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthApeShape)},
                {"MouthUpperUpLeftPout", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthPout)},
                {"MouthUpperUpLeftOverlay", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthLowerOverlay)},
                {"MouthUpperUpLeftSuck", new PositiveNegativeShape(UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.CheekSuck)},

                // MouthUpperUp Left+Right base params
                {"MouthUpperUpUpperInside", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperInside })},
                {"MouthUpperUpInside", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperInside, UnifiedExpression.MouthLowerInside}, true)},
                {"MouthUpperUpPuff", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft, UnifiedExpression.CheekPuffRight})},
                {"MouthUpperUpPuffLeft", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft})},
                {"MouthUpperUpPuffRight", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffRight})},
                {"MouthUpperUpApe", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.MouthApeShape})},
                {"MouthUpperUpPout", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.MouthPout})},
                {"MouthUpperUpOverlay", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.MouthLowerOverlay})},
                {"MouthUpperUpSuck", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthUpperUpLeft, UnifiedExpression.MouthUpperUpRight}, new UnifiedExpression[]{UnifiedExpression.CheekSuck})},

                //MouthLowerDownRight based params
                {"MouthLowerDownRightLowerInside", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.MouthLowerInside)},
                {"MouthLowerDownRightPuffRight", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.CheekPuffRight)},
                {"MouthLowerDownRightApe", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.MouthApeShape)},
                {"MouthLowerDownRightPout", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.MouthPout)},
                {"MouthLowerDownRightOverlay", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.MouthLowerOverlay)},
                {"MouthLowerDownRightSuck", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownRight, UnifiedExpression.CheekSuck)},

                //MouthLowerDownLeft based params
                {"MouthLowerDownLeftLowerInside", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerInside)},
                {"MouthLowerDownLeftPuffLeft", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.CheekPuffLeft)},
                {"MouthLowerDownLeftApe", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthApeShape)},
                {"MouthLowerDownLeftPout", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthPout)},
                {"MouthLowerDownLeftOverlay", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerOverlay)},
                {"MouthLowerDownLeftSuck", new PositiveNegativeShape(UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.CheekSuck)},

                // MouthLowerDown Left+Right base params
                {"MouthLowerDownLowerInside", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.MouthLowerInside})},
                {"MouthLowerDownInside", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperInside, UnifiedExpression.MouthLowerInside}, true)},
                {"MouthLowerDownPuff", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft, UnifiedExpression.CheekPuffRight})},
                {"MouthLowerDownPuffLeft", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft})},
                {"MouthLowerDownPuffRight", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.CheekPuffRight})},
                {"MouthLowerDownApe", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.MouthApeShape})},
                {"MouthLowerDownPout", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.MouthPout})},
                {"MouthLowerDownOverlay", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.MouthLowerOverlay})},
                {"MouthLowerDownSuck", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthLowerDownLeft, UnifiedExpression.MouthLowerDownRight}, new UnifiedExpression[]{UnifiedExpression.CheekSuck})},

				// MouthInsideOverturn based params
				{"MouthUpperInsideOverturn", new PositiveNegativeShape(UnifiedExpression.MouthUpperInside, UnifiedExpression.MouthUpperOverturn)},
				{"MouthLowerInsideOverturn", new PositiveNegativeShape(UnifiedExpression.MouthLowerInside, UnifiedExpression.MouthLowerOverturn)},
				
                //SmileRight based params; Recommend using these if you already have SmileSadLeft setup!
                {"SmileRightUpperOverturn", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthUpperOverturn)},
                {"SmileRightLowerOverturn", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthLowerOverturn)},
                {"SmileRightOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn})},
                {"SmileRightApe", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthApeShape)},
                {"SmileRightOverlay", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthLowerOverlay)},
                {"SmileRightPout", new PositiveNegativeShape(UnifiedExpression.MouthSmileRight, UnifiedExpression.MouthPout)},

                //SmileLeft based params; Recommend using these if you already have SmileSadRight setup!
                {"SmileLeftUpperOverturn", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthUpperOverturn)},
                {"SmileLeftLowerOverturn", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthLowerOverturn)},
                {"SmileLeftOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn})},
                {"SmileLeftApe", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthApeShape)},
                {"SmileLeftOverlay", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthLowerOverlay)},
                {"SmileLeftPout", new PositiveNegativeShape(UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthPout)},

                //Smile Left+Right based params
                {"SmileUpperOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn})},
                {"SmileLowerOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthLowerOverturn})},
                {"SmileOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn})},
                {"SmileApe", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthApeShape})},
                {"SmileOverlay", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthLowerOverlay})},
                {"SmilePout", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.MouthSmileLeft, UnifiedExpression.MouthSmileRight}, new UnifiedExpression[]{UnifiedExpression.MouthPout})},

                //CheekPuffRight based params
                {"PuffRightUpperOverturn", new PositiveNegativeShape(UnifiedExpression.CheekPuffRight, UnifiedExpression.MouthUpperOverturn)},
                {"PuffRightLowerOverturn", new PositiveNegativeShape(UnifiedExpression.CheekPuffRight, UnifiedExpression.MouthLowerOverturn)},
                {"PuffRightOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffRight}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn}, true)},

                //CheekPuffLeft based params
                {"PuffLeftUpperOverturn", new PositiveNegativeShape(UnifiedExpression.CheekPuffLeft, UnifiedExpression.MouthUpperOverturn)},
                {"PuffLeftLowerOverturn", new PositiveNegativeShape(UnifiedExpression.CheekPuffLeft, UnifiedExpression.MouthLowerOverturn)},
                {"PuffLeftOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffLeft}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn}, true)},

                //CheekPuff Left+Right based params
                {"PuffUpperOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffRight, UnifiedExpression.CheekPuffLeft}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn})},
                {"PuffLowerOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffRight, UnifiedExpression.CheekPuffLeft}, new UnifiedExpression[]{UnifiedExpression.MouthLowerOverturn})},
                {"PuffOverturn", new PositiveNegativeAveragedShape(new UnifiedExpression[]{UnifiedExpression.CheekPuffRight, UnifiedExpression.CheekPuffLeft}, new UnifiedExpression[]{UnifiedExpression.MouthUpperOverturn, UnifiedExpression.MouthLowerOverturn}, true)},

                //Combine both TongueSteps (-1 fully in, 0 on edge, 1 fully out)
                {"TongueSteps", new PositiveNegativeShape(UnifiedExpression.TongueLongStep1, UnifiedExpression.TongueLongStep2, true)},
            };
        
        // Make a list called LipParameters containing the results from both GetOptimizedLipParameters and GetAllLipParameters, and add GetLipActivatedStatus
        public static readonly IParameter[] AllLipParameters =
            GetAllLipShapes().Union(GetOptimizedLipParameters()).Union(GetLipActivatedStatus()).ToArray();

        public static bool IsLipShapeName(string name) => MergedShapes.ContainsKey(name) || Enum.TryParse(name, out UnifiedExpression shape);
        
        private static IEnumerable<EParam> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new EParam((eye, lip) => 
                shape.Value.GetBlendedLipShape(lip.LatestShapes), shape.Key, 0.0f));

        private static IEnumerable<EParam> GetAllLipShapes() =>
            ((UnifiedExpression[]) Enum.GetValues(typeof(UnifiedExpression))).ToList().Select(shape =>
                new EParam((eye, lip) => lip.LatestShapes[(int)shape],
                    shape.ToString(), 0.0f));

        private static IEnumerable<IParameter> GetLipActivatedStatus() => new List<IParameter>
        {
            new BoolParameter(v2 => UnifiedLibManager.LipStatus.Equals(ModuleState.Active), "LipTrackingActive"),
        };
    }
}