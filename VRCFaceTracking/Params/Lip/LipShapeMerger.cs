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
                {"JawX", new PositiveNegativeShape(VRCFTLipShape.JawRight, VRCFTLipShape.JawLeft)},
                {"MouthUpper", new PositiveNegativeShape(VRCFTLipShape.MouthUpperRight, VRCFTLipShape.MouthUpperLeft)},
                {"MouthLower", new PositiveNegativeShape(VRCFTLipShape.MouthLowerRight, VRCFTLipShape.MouthLowerLeft)},
                {"MouthX", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperRight, VRCFTLipShape.MouthLowerRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperLeft, VRCFTLipShape.MouthLowerLeft}, true)},
                {"SmileSadRight", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthSadRight)},
                {"SmileSadLeft", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSadLeft)},
                {"SmileSad", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthSadLeft, VRCFTLipShape.MouthSadRight})},
                {"TongueY", new PositiveNegativeShape(VRCFTLipShape.TongueUp, VRCFTLipShape.TongueDown)},
                {"TongueX", new PositiveNegativeShape(VRCFTLipShape.TongueRight, VRCFTLipShape.TongueLeft)},
                {"PuffSuckRight", new PositiveNegativeShape(VRCFTLipShape.CheekPuffRight, VRCFTLipShape.CheekSuck)},
                {"PuffSuckLeft", new PositiveNegativeShape(VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.CheekSuck)},
                {"PuffSuck", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.CheekPuffRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekSuck}, true)},

                //Additional combined shapes created with the help of the VRCFT Discord!

                //JawOpen based params
                {"JawOpenApe", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.MouthApeShape)},
                {"JawOpenPuff", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.JawOpen}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.CheekPuffRight})},
                {"JawOpenPuffRight", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.CheekPuffRight)},
                {"JawOpenPuffLeft", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.CheekPuffLeft)},
                {"JawOpenSuck", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.CheekSuck)},
                {"JawOpenForward", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.JawForward)},
                {"JawOpenOverlay", new PositiveNegativeShape(VRCFTLipShape.JawOpen, VRCFTLipShape.MouthLowerOverlay)},

                //MouthUpperUpRight based params
                {"MouthUpperUpRightUpperInside", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.MouthUpperInside)},
                {"MouthUpperUpRightPuffRight", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.CheekPuffRight)},
                {"MouthUpperUpRightApe", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.MouthApeShape)},
                {"MouthUpperUpRightPout", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.MouthPout)},
                {"MouthUpperUpRightOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.MouthLowerOverlay)},
                {"MouthUpperUpRightSuck", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpRight, VRCFTLipShape.CheekSuck)},
                
                //MouthUpperUpLeft based params
                {"MouthUpperUpLeftUpperInside", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperInside)},
                {"MouthUpperUpLeftPuffLeft", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.CheekPuffLeft)},
                {"MouthUpperUpLeftApe", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthApeShape)},
                {"MouthUpperUpLeftPout", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthPout)},
                {"MouthUpperUpLeftOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthLowerOverlay)},
                {"MouthUpperUpLeftSuck", new PositiveNegativeShape(VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.CheekSuck)},

                // MouthUpperUp Left+Right base params
                {"MouthUpperUpUpperInside", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperInside })},
                {"MouthUpperUpInside", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperInside, VRCFTLipShape.MouthLowerInside}, true)},
                {"MouthUpperUpPuff", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.CheekPuffRight})},
                {"MouthUpperUpPuffLeft", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft})},
                {"MouthUpperUpPuffRight", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight})},
                {"MouthUpperUpApe", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthApeShape})},
                {"MouthUpperUpPout", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthPout})},
                {"MouthUpperUpOverlay", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerOverlay})},
                {"MouthUpperUpSuck", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthUpperUpLeft, VRCFTLipShape.MouthUpperUpRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekSuck})},

                //MouthLowerDownRight based params
                {"MouthLowerDownRightLowerInside", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.MouthLowerInside)},
                {"MouthLowerDownRightPuffRight", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.CheekPuffRight)},
                {"MouthLowerDownRightApe", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.MouthApeShape)},
                {"MouthLowerDownRightPout", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.MouthPout)},
                {"MouthLowerDownRightOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.MouthLowerOverlay)},
                {"MouthLowerDownRightSuck", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownRight, VRCFTLipShape.CheekSuck)},

                //MouthLowerDownLeft based params
                {"MouthLowerDownLeftLowerInside", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerInside)},
                {"MouthLowerDownLeftPuffLeft", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.CheekPuffLeft)},
                {"MouthLowerDownLeftApe", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthApeShape)},
                {"MouthLowerDownLeftPout", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthPout)},
                {"MouthLowerDownLeftOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerOverlay)},
                {"MouthLowerDownLeftSuck", new PositiveNegativeShape(VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.CheekSuck)},

                // MouthLowerDown Left+Right base params
                {"MouthLowerDownLowerInside", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerInside})},
                {"MouthLowerDownInside", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperInside, VRCFTLipShape.MouthLowerInside}, true)},
                {"MouthLowerDownPuff", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.CheekPuffRight})},
                {"MouthLowerDownPuffLeft", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft})},
                {"MouthLowerDownPuffRight", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight})},
                {"MouthLowerDownApe", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthApeShape})},
                {"MouthLowerDownPout", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthPout})},
                {"MouthLowerDownOverlay", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerOverlay})},
                {"MouthLowerDownSuck", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthLowerDownLeft, VRCFTLipShape.MouthLowerDownRight}, new VRCFTLipShape[]{VRCFTLipShape.CheekSuck})},

				// MouthInsideOverturn based params
				{"MouthUpperInsideOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthUpperInside, VRCFTLipShape.MouthUpperOverturn)},
				{"MouthLowerInsideOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthLowerInside, VRCFTLipShape.MouthLowerOverturn)},
				
                //SmileRight based params; Recommend using these if you already have SmileSadLeft setup!
                {"SmileRightUpperOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthUpperOverturn)},
                {"SmileRightLowerOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthLowerOverturn)},
                {"SmileRightOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn})},
                {"SmileRightApe", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthApeShape)},
                {"SmileRightOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthLowerOverlay)},
                {"SmileRightPout", new PositiveNegativeShape(VRCFTLipShape.MouthSmileRight, VRCFTLipShape.MouthPout)},

                //SmileLeft based params; Recommend using these if you already have SmileSadRight setup!
                {"SmileLeftUpperOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthUpperOverturn)},
                {"SmileLeftLowerOverturn", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthLowerOverturn)},
                {"SmileLeftOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn})},
                {"SmileLeftApe", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthApeShape)},
                {"SmileLeftOverlay", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthLowerOverlay)},
                {"SmileLeftPout", new PositiveNegativeShape(VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthPout)},

                //Smile Left+Right based params
                {"SmileUpperOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn})},
                {"SmileLowerOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerOverturn})},
                {"SmileOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn})},
                {"SmileApe", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthApeShape})},
                {"SmileOverlay", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerOverlay})},
                {"SmilePout", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.MouthSmileLeft, VRCFTLipShape.MouthSmileRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthPout})},

                //CheekPuffRight based params
                {"PuffRightUpperOverturn", new PositiveNegativeShape(VRCFTLipShape.CheekPuffRight, VRCFTLipShape.MouthUpperOverturn)},
                {"PuffRightLowerOverturn", new PositiveNegativeShape(VRCFTLipShape.CheekPuffRight, VRCFTLipShape.MouthLowerOverturn)},
                {"PuffRightOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn}, true)},

                //CheekPuffLeft based params
                {"PuffLeftUpperOverturn", new PositiveNegativeShape(VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.MouthUpperOverturn)},
                {"PuffLeftLowerOverturn", new PositiveNegativeShape(VRCFTLipShape.CheekPuffLeft, VRCFTLipShape.MouthLowerOverturn)},
                {"PuffLeftOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffLeft}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn}, true)},

                //CheekPuff Left+Right based params
                {"PuffUpperOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight, VRCFTLipShape.CheekPuffLeft}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn})},
                {"PuffLowerOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight, VRCFTLipShape.CheekPuffLeft}, new VRCFTLipShape[]{VRCFTLipShape.MouthLowerOverturn})},
                {"PuffOverturn", new PositiveNegativeAveragedShape(new VRCFTLipShape[]{VRCFTLipShape.CheekPuffRight, VRCFTLipShape.CheekPuffLeft}, new VRCFTLipShape[]{VRCFTLipShape.MouthUpperOverturn, VRCFTLipShape.MouthLowerOverturn}, true)},

                //Combine both TongueSteps (-1 fully in, 0 on edge, 1 fully out)
                {"TongueSteps", new PositiveNegativeShape(VRCFTLipShape.TongueLongStep1, VRCFTLipShape.TongueLongStep2, true)},
            };
        
        // Make a list called LipParameters containing the results from both GetOptimizedLipParameters and GetAllLipParameters, and add GetLipActivatedStatus
        public static readonly IParameter[] AllLipParameters =
            GetAllLipShapes().Union(GetOptimizedLipParameters()).Union(GetLipActivatedStatus()).ToArray();

        public static bool IsLipShapeName(string name) => MergedShapes.ContainsKey(name) || Enum.TryParse(name, out VRCFTLipShape shape);
        
        private static IEnumerable<EParam> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new EParam((eye, lip) => 
                shape.Value.GetBlendedLipShape(lip.LatestShapes), shape.Key, 0.0f));

        private static IEnumerable<EParam> GetAllLipShapes() =>
            ((VRCFTLipShape[]) Enum.GetValues(typeof(VRCFTLipShape))).ToList().Select(shape =>
                new EParam((eye, lip) => lip.LatestShapes[(int)shape],
                    shape.ToString(), 0.0f));

        private static IEnumerable<IParameter> GetLipActivatedStatus() => new List<IParameter>
        {
            new BoolParameter(v2 => UnifiedLibManager.LipStatus.Equals(ModuleState.Active), "LipTrackingActive"),
        };
    }
}