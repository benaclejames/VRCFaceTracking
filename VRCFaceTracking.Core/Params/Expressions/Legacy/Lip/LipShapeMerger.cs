using VRCFaceTracking.Core.Params.DataTypes;

namespace VRCFaceTracking.Core.Params.Expressions.Legacy.Lip
{
    [Obsolete("SRanipal Lip Shapes are being phased out. Please switch to Unified Expressions.")]
    public static class LipShapeMerger
    {
        private static readonly Dictionary<string, ICombinedShape> MergedShapes =
            new Dictionary<string, ICombinedShape>
            {
                {"JawX", new PositiveNegativeShape(SRanipal_LipShape_v2.JawRight, SRanipal_LipShape_v2.JawLeft)},
                {"MouthUpper", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperRight, SRanipal_LipShape_v2.MouthUpperLeft)},
                {"MouthLower", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerRight, SRanipal_LipShape_v2.MouthLowerLeft)},
                {"MouthX", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperRight, SRanipal_LipShape_v2.MouthLowerRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperLeft, SRanipal_LipShape_v2.MouthLowerLeft}, true)},
                {"SmileSadRight", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthSadRight)},
                {"SmileSadLeft", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSadLeft)},
                {"SmileSad", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSadLeft, SRanipal_LipShape_v2.MouthSadRight})},
                {"TongueY", new PositiveNegativeShape(SRanipal_LipShape_v2.TongueUp, SRanipal_LipShape_v2.TongueDown)},
                {"TongueX", new PositiveNegativeShape(SRanipal_LipShape_v2.TongueRight, SRanipal_LipShape_v2.TongueLeft)},
                {"PuffSuckRight", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.CheekSuck)},
                {"PuffSuckLeft", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.CheekSuck)},
                {"PuffSuck", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.CheekPuffRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekSuck}, true)},

                //Additional combined shapes created with the help of the VRCFT Discord!

                //JawOpen based params
                {"JawOpenApe", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.MouthApeShape)},
                {"JawOpenPuff", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.JawOpen}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.CheekPuffRight})},
                {"JawOpenPuffRight", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.CheekPuffRight)},
                {"JawOpenPuffLeft", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.CheekPuffLeft)},
                {"JawOpenSuck", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.CheekSuck)},
                {"JawOpenForward", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.JawForward)},
                {"JawOpenOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.JawOpen, SRanipal_LipShape_v2.MouthLowerOverlay)},

                //MouthUpperUpRight based params
                {"MouthUpperUpRightUpperInside", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.MouthUpperInside)},
                {"MouthUpperUpRightPuffRight", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.CheekPuffRight)},
                {"MouthUpperUpRightApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.MouthApeShape)},
                {"MouthUpperUpRightPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.MouthPout)},
                {"MouthUpperUpRightOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"MouthUpperUpRightSuck", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpRight, SRanipal_LipShape_v2.CheekSuck)},
                
                //MouthUpperUpLeft based params
                {"MouthUpperUpLeftUpperInside", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperInside)},
                {"MouthUpperUpLeftPuffLeft", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.CheekPuffLeft)},
                {"MouthUpperUpLeftApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthApeShape)},
                {"MouthUpperUpLeftPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthPout)},
                {"MouthUpperUpLeftOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"MouthUpperUpLeftSuck", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.CheekSuck)},

                // MouthUpperUp Left+Right base params
                {"MouthUpperUpUpperInside", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperInside })},
                {"MouthUpperUpInside", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperInside, SRanipal_LipShape_v2.MouthLowerInside}, true)},
                {"MouthUpperUpPuff", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.CheekPuffRight})},
                {"MouthUpperUpPuffLeft", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft})},
                {"MouthUpperUpPuffRight", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight})},
                {"MouthUpperUpApe", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthApeShape})},
                {"MouthUpperUpPout", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthPout})},
                {"MouthUpperUpOverlay", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerOverlay})},
                {"MouthUpperUpSuck", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperUpLeft, SRanipal_LipShape_v2.MouthUpperUpRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekSuck})},

                //MouthLowerDownRight based params
                {"MouthLowerDownRightLowerInside", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.MouthLowerInside)},
                {"MouthLowerDownRightPuffRight", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.CheekPuffRight)},
                {"MouthLowerDownRightApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.MouthApeShape)},
                {"MouthLowerDownRightPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.MouthPout)},
                {"MouthLowerDownRightOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"MouthLowerDownRightSuck", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownRight, SRanipal_LipShape_v2.CheekSuck)},

                //MouthLowerDownLeft based params
                {"MouthLowerDownLeftLowerInside", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerInside)},
                {"MouthLowerDownLeftPuffLeft", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.CheekPuffLeft)},
                {"MouthLowerDownLeftApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthApeShape)},
                {"MouthLowerDownLeftPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthPout)},
                {"MouthLowerDownLeftOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"MouthLowerDownLeftSuck", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.CheekSuck)},

                // MouthLowerDown Left+Right base params
                {"MouthLowerDownLowerInside", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerInside})},
                {"MouthLowerDownInside", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperInside, SRanipal_LipShape_v2.MouthLowerInside}, true)},
                {"MouthLowerDownPuff", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.CheekPuffRight})},
                {"MouthLowerDownPuffLeft", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft})},
                {"MouthLowerDownPuffRight", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight})},
                {"MouthLowerDownApe", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthApeShape})},
                {"MouthLowerDownPout", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthPout})},
                {"MouthLowerDownOverlay", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerOverlay})},
                {"MouthLowerDownSuck", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerDownLeft, SRanipal_LipShape_v2.MouthLowerDownRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekSuck})},

				// MouthInsideOverturn based params
				{"MouthUpperInsideOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthUpperInside, SRanipal_LipShape_v2.MouthUpperOverturn)},
				{"MouthLowerInsideOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthLowerInside, SRanipal_LipShape_v2.MouthLowerOverturn)},
				
                //SmileRight based params; Recommend using these if you already have SmileSadLeft setup!
                {"SmileRightUpperOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthUpperOverturn)},
                {"SmileRightLowerOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthLowerOverturn)},
                {"SmileRightOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn})},
                {"SmileRightApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthApeShape)},
                {"SmileRightOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"SmileRightPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileRight, SRanipal_LipShape_v2.MouthPout)},

                //SmileLeft based params; Recommend using these if you already have SmileSadRight setup!
                {"SmileLeftUpperOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthUpperOverturn)},
                {"SmileLeftLowerOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthLowerOverturn)},
                {"SmileLeftOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn})},
                {"SmileLeftApe", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthApeShape)},
                {"SmileLeftOverlay", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthLowerOverlay)},
                {"SmileLeftPout", new PositiveNegativeShape(SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthPout)},

                //Smile Left+Right based params
                {"SmileUpperOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn})},
                {"SmileLowerOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerOverturn})},
                {"SmileOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn})},
                {"SmileApe", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthApeShape})},
                {"SmileOverlay", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerOverlay})},
                {"SmilePout", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthSmileLeft, SRanipal_LipShape_v2.MouthSmileRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthPout})},

                //CheekPuffRight based params
                {"PuffRightUpperOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.MouthUpperOverturn)},
                {"PuffRightLowerOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.MouthLowerOverturn)},
                {"PuffRightOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn}, true)},

                //CheekPuffLeft based params
                {"PuffLeftUpperOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.MouthUpperOverturn)},
                {"PuffLeftLowerOverturn", new PositiveNegativeShape(SRanipal_LipShape_v2.CheekPuffLeft, SRanipal_LipShape_v2.MouthLowerOverturn)},
                {"PuffLeftOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffLeft}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn}, true)},

                //CheekPuff Left+Right based params
                {"PuffUpperOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.CheekPuffLeft}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn})},
                {"PuffLowerOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.CheekPuffLeft}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthLowerOverturn})},
                {"PuffOverturn", new PositiveNegativeAveragedShape(new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.CheekPuffRight, SRanipal_LipShape_v2.CheekPuffLeft}, new SRanipal_LipShape_v2[]{SRanipal_LipShape_v2.MouthUpperOverturn, SRanipal_LipShape_v2.MouthLowerOverturn}, true)},

                //Combine both TongueSteps (-1 fully in, 0 on edge, 1 fully out)
                {"TongueSteps", new PositiveNegativeShape(SRanipal_LipShape_v2.TongueLongStep1, SRanipal_LipShape_v2.TongueLongStep2, true)},
            };

        // Make a list called LipParameters containing the results from both GetOptimizedLipParameters and GetAllLipParameters, and add GetLipActivatedStatus
        public static readonly Parameter[] AllLipParameters =
            GetAllLipShapes().Union(GetOptimizedLipParameters()).ToArray();

        private static IEnumerable<EParam> GetOptimizedLipParameters() => MergedShapes
            .Select(shape => new EParam(shape.Key, exp => 
                shape.Value.GetBlendedLipShape(exp), 0.0f));

        private static IEnumerable<EParam> GetAllLipShapes() =>
            ((SRanipal_LipShape_v2[])Enum.GetValues(typeof(SRanipal_LipShape_v2))).ToList().Select(shape =>
               new EParam(shape.ToString(), exp => UnifiedSRanMapper.GetTransformedShape(shape, exp), 0.0f));
    }
}