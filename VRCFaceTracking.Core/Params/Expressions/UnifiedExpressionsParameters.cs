using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.DataTypes;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Params.Expressions;

public static class UnifiedExpressionsParameters
{
    private static (string paramName, Parameter paramLiteral)[] IsEyeParameter(IParameterDefinition[] param)
    {
        // Get all the names of all parameters in both the unified tracking list and the old legacy eye list
        var allParams = UnifiedTracking.AllParameters_v2.Concat(EyeTrackingParams.ParameterList).ToList()
            .SelectMany(p => p.GetParamNames());
                
        // Now we match parameters to the literals as a sort of sanity check
        return allParams.Where(p => param.Any(p2 => p2.Address.EndsWith(p.paramName))).ToArray();
    }
    
    public static readonly Parameter[] UnifiedCombinedShapes =
    {    
        // Unified Eye Definitions
        
        #region Eye Gaze
        
        new EParam("v2/Eye", exp => exp.Eye.Combined().Gaze),
        new EParam("v2/EyeLeft", exp => exp.Eye.Left.Gaze),
        new EParam("v2/EyeRight", exp => exp.Eye.Right.Gaze),
        
        // Use when tracking interface is sending verbose gaze data.
        new NativeParameter<Vector2>(exp =>
            new Vector2(exp.Eye.Combined().Gaze.ToPitch(), 
                        exp.Eye.Combined().Gaze.ToYaw()),
            param => 
                IsEyeParameter(
                    param.Where(p => 
                    p.Name.Contains("Eye") && 
                    (p.Name.Contains("Left") || p.Name.Contains("Right") || p.Name.Contains("Eyes")) && 
                    (p.Name.Contains('X') || p.Name.Contains('Y'))).ToArray()
                )
                .Length == 0,
            "/tracking/eye/CenterPitchYaw"
            ),
        /*
        // Use when tracking interface is sending combined gaze data.
        new NativeParameter<Vector4>(exp =>
            new Vector4(exp.Eye.Right.Gaze.ToPitch(), 
                        exp.Eye.Right.Gaze.ToYaw(), 
                        exp.Eye.Left.Gaze.ToPitch(), 
                        exp.Eye.Left.Gaze.ToYaw()),
            param => 
                IsEyeParameter(
                param.Where(p =>
                    p.name.Contains("Eye") &&
                    (p.name.Contains('X') || p.name.Contains('Y'))).ToArray())
                    .Length == 0,
            "/tracking/eye/LeftRightPitchYaw" // THE INPUT IS BACKWARDSSSSS
        ),
        */

        new NativeParameter<float>(
            exp => 1 - exp.Eye.Combined().Openness,
            param => IsEyeParameter(
                    param.Where(p =>
                        p.Name.Contains("Eye") &&
                (p.Name.Contains("Open") || p.Name.Contains("Lid"))).ToArray())
                .Length == 0,
            "/tracking/eye/EyesClosedAmount"
        ),
        
        #endregion

        #region Eye Pupils
        
        new EParam("v2/PupilDilation", exp => exp.Eye.Combined().PupilDiameter_MM),

        new EParam("v2/PupilDiameterLeft", exp => exp.Eye.Left.PupilDiameter_MM * 0.1f),
        new EParam("v2/PupilDiameterRight", exp => exp.Eye.Right.PupilDiameter_MM * 0.1f),
        new EParam("v2/PupilDiameter", exp => (exp.Eye.Left.PupilDiameter_MM * 0.1f + exp.Eye.Left.PupilDiameter_MM * 0.1f) / 2.0f),
        
        #endregion

        #region Eye Openness
        
        new EParam("v2/EyeOpenLeft" , exp => exp.Eye.Left.Openness),
        new EParam("v2/EyeOpenRight", exp => exp.Eye.Right.Openness),
        new EParam("v2/EyeOpen", exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f),
        
        new EParam("v2/EyeClosedLeft" , exp => 1 - exp.Eye.Left.Openness),
        new EParam("v2/EyeClosedRight", exp => 1 - exp.Eye.Right.Openness),
        new EParam("v2/EyeClosed", exp => 1 - (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f),

        #endregion

        #region Eyes Widen (Combined)

        new EParam("v2/EyeWide", exp =>
            exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight),

        #endregion

        #region Eyelids Combined

        new EParam("v2/EyeLidLeft", exp => exp.Eye.Left.Openness * .75f + exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .25f),
        new EParam("v2/EyeLidRight", exp => exp.Eye.Right.Openness * .75f + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .25f),
        new EParam("v2/EyeLid", exp =>
            ((exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f) * .75f +
            ((exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight) / 2.0f) * .25f),
       
        #endregion

        #region Eyes Squint (Combined)

        new EParam("v2/EyeSquint", exp =>
            exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight),

        new EParam("v2/EyesSquint", exp =>
            exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight),
        #endregion

        #region Eyebrows Compacted

        new EParam("v2/BrowUp", exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowUpRight) + GetSimpleShape(exp, UnifiedSimpleExpressions.BrowUpLeft)),
        new EParam("v2/BrowDown", exp => 
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight) + GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft)),

        new EParam("v2/BrowInnerUp", exp => 
            (exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight) / 2.0f),
        new EParam("v2/BrowOuterUp", exp => 
            (exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight) / 2.0f),

        // -1 = 'Angry', +1 = 'Worried'
        new EParam("v2/BrowExpressionRight", exp => 
            Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight * .5f) -
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight)),

        new EParam("v2/BrowExpressionLeft", exp => 
            Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight * .5f) -
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft)),

        new EParam("v2/BrowExpression", exp =>
            (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight * .5f) -
             Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight * .5f)) +
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight) * .5f - GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft) * .5f),

        #endregion

        #region Jaw Combined

        new EParam("v2/JawX", exp => exp.Shapes[(int)UnifiedExpressions.JawRight].Weight - exp.Shapes[(int)UnifiedExpressions.JawLeft].Weight),
        new EParam("v2/JawZ", exp => exp.Shapes[(int)UnifiedExpressions.JawForward].Weight - exp.Shapes[(int)UnifiedExpressions.JawBackward].Weight),

        #endregion

        #region Cheeks Combined

        new EParam("v2/CheekSquint", exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2.0f),

        new EParam("v2/CheekPuffSuckLeft", exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight),
        new EParam("v2/CheekPuffSuckRight", exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight),

        new EParam("v2/CheekPuffSuck", exp =>
            (exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight + exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2.0f),

        new EParam("v2/CheekSuck", exp => (exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2.0f),

        #endregion

        #region Mouth Direction

        new EParam("v2/MouthUpperX", exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].Weight),
        new EParam("v2/MouthLowerX", exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].Weight),

        new EParam("v2/MouthX", exp =>
            (exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].Weight) / 2.0f),

        #endregion

        #region Lip Combined

        new EParam("v2/LipSuckUpper", exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight) / 2.0f),
        new EParam("v2/LipSuckLower", exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight) / 2.0f),
        new EParam("v2/LipSuck", exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
             exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight) / 4.0f),

        new EParam("v2/LipFunnelUpper", exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight) / 2.0f),
        new EParam("v2/LipFunnelLower", exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 2.0f),
        new EParam("v2/LipFunnel", exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight + 
             exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 4.0f),
        new EParam("v2/LipPuckerUpper", exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight) / 2.0f),
        new EParam("v2/LipPuckerLower", exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2.0f),
        new EParam("v2/LipPuckerRight", exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2.0f),
        new EParam("v2/LipPuckerLeft", exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2.0f),
        new EParam("v2/LipPucker", exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
             exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 4.0f),

        // Compacted paramaters

        new EParam("v2/LipSuckFunnelUpper", exp =>
            (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight) / 2.0f),

        new EParam("v2/LipSuckFunnelLower", exp =>
            (exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 2.0f),

        new EParam("v2/LipSuckFunnelLowerLeft", exp => exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight - exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight),
        new EParam("v2/LipSuckFunnelLowerRight", exp => exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight - exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight),
        new EParam("v2/LipSuckFunnelUpperLeft", exp => exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight - exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight),
        new EParam("v2/LipSuckFunnelUpperRight", exp => exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight - exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight),

        #endregion

        #region Mouth Combined
        
        new EParam("v2/MouthUpperUp", exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * .5f),
        new EParam("v2/MouthLowerDown", exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * .5f + exp.Shapes[(int) UnifiedExpressions.MouthLowerDownLeft].Weight * .5f),
        new EParam("v2/MouthOpen", exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * .25f + exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * .25f +
            exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * .25f + exp.Shapes[(int) UnifiedExpressions.MouthLowerDownLeft].Weight * .25f),

        new EParam("v2/MouthStretch", exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight) / 2.0f),
        new EParam("v2/MouthTightener", exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight) / 2.0f),
        new EParam("v2/MouthPress", exp => (exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight) / 2.0f),
        new EParam("v2/MouthDimple", exp => (exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight) / 2.0f),

        new EParam("v2/NoseSneer", exp => (exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight + exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight) / 2.0f),

        // Compacted paramamters

        new EParam("v2/MouthTightenerStretch", exp =>
            (exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight) / 2.0f),

        new EParam("v2/MouthTightenerStretchLeft", exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight - exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight),
        new EParam("v2/MouthTightenerStretchRight", exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight),
        #endregion

        #region Lip Corners Combined

        new EParam("v2/MouthCornerYLeft", exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight),
        new EParam("v2/MouthCornerYRight", exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight),
        new EParam("v2/MouthCornerY", exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight * .5f -
            exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight),

        new EParam("v2/SmileFrownRight", exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) - 
            exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight),
        new EParam("v2/SmileFrownLeft", exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) - 
            exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight),

        new EParam("v2/SmileFrown", exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) * .5f + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) * .5f -
            exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight * .5f - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight * .5f),

        // Smile 'Sad' contains both the stretcher and frown shapes to represent sad (similar in functionality to SRanipal Sad, just with explicit acknowledgment of lessened tracking fidelity).
        new EParam("v2/SmileSadRight", exp => 
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) - GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadRight)),

        new EParam("v2/SmileSadLeft", exp => 
            GetSimpleShape(exp,
                UnifiedSimpleExpressions.MouthSmileLeft) - GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadLeft)),

        new EParam("v2/SmileSad", exp =>
            (GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight)) / 2.0f -
            (GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadLeft) + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadRight)) / 2.0f),

        #endregion 

        #region Tongue Combined

        new EParam("v2/TongueX", exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].Weight - exp.Shapes[(int)UnifiedExpressions.TongueLeft].Weight),
        new EParam("v2/TongueY", exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].Weight - exp.Shapes[(int)UnifiedExpressions.TongueDown].Weight),
        new EParam("v2/TongueArchY", exp => exp.Shapes[(int)UnifiedExpressions.TongueCurlUp].Weight - exp.Shapes[(int)UnifiedExpressions.TongueBendDown].Weight),
        new EParam("v2/TongueShape", exp => exp.Shapes[(int)UnifiedExpressions.TongueFlat].Weight - exp.Shapes[(int)UnifiedExpressions.TongueSquish].Weight),

        #endregion

        new ConditionalBoolParameter(exp => (UnifiedLibManager.EyeStatus == ModuleState.Active, UnifiedLibManager.EyeStatus != ModuleState.Uninitialized), "EyeTrackingActive"),
        new ConditionalBoolParameter(exp => (UnifiedLibManager.ExpressionStatus == ModuleState.Active, UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized), "ExpressionTrackingActive"),
        new ConditionalBoolParameter(exp => (UnifiedLibManager.ExpressionStatus == ModuleState.Active, UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized), "LipTrackingActive")

    };

    public static readonly Parameter[] ExpressionParameters =
        GetAllBaseExpressions().Union(GetAllBaseSimpleExpressions()).Union(UnifiedCombinedShapes).ToArray();

    private static IEnumerable<EParam> GetAllBaseExpressions() =>
        ((UnifiedExpressions[])Enum.GetValues(typeof(UnifiedExpressions))).ToList().Select(shape =>
           new EParam("v2/" + shape.ToString(), exp => exp.Shapes[(int)shape].Weight, 0.0f));
    private static IEnumerable<EParam> GetAllBaseSimpleExpressions() =>
        ((UnifiedSimpleExpressions[])Enum.GetValues(typeof(UnifiedSimpleExpressions))).ToList().Select(simple =>
           new EParam("v2/" + simple.ToString(), exp => GetSimpleShape(exp, simple), 0.0f));

    private static float GetSimpleShape(UnifiedTrackingData data, UnifiedSimpleExpressions expression) => UnifiedSimplifier.ExpressionMap[expression].Invoke(data);
}