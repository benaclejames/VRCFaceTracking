using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Params.Expressions;

public static class UnifiedExpressionsMerger
{
    private static (string paramName, IParameter paramLiteral)[] IsEyeParameter(ConfigParser.Parameter[] param)
    {
        // Get all the names of all parameters in both the unified tracking list and the old legacy eye list
        var allParams = UnifiedTracking.AllParameters_v2.Concat(EyeTrackingParams.ParameterList).ToList()
            .SelectMany(p => p.GetParamNames());
                
        // Now we match parameters to the literals as a sort of sanity check
        return allParams.Where(p => param.Any(p2 => p.paramName == p2.name)).ToArray();
    }
    
    public static readonly IParameter[] UnifiedCombinedShapes =
    {    
        // Unified Eye Definitions
        
        #region Eye Gaze
        
        new EParam(exp => exp.Eye.Combined().Gaze, "v2/Eye"),
        new EParam(exp => exp.Eye.Left.Gaze, "v2/EyeLeft"),
        new EParam(exp => exp.Eye.Right.Gaze, "v2/EyeRight"),
        
        // Use when tracking interface is sending verbose gaze data.
        new NativeParameter<Vector2>(exp =>
            new Vector2(exp.Eye.Combined().Gaze.ToPitch(), 
                        exp.Eye.Combined().Gaze.ToYaw()),
            param => 
                IsEyeParameter(
                param.Where(p => 
                p.name.Contains("Eye") && 
                (p.name.Contains("Left") || p.name.Contains("Right") || p.name.Contains("Eyes")) && 
                (p.name.Contains('X') || p.name.Contains('Y'))).ToArray())
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
                        p.name.Contains("Eye") &&
                (p.name.Contains("Open") || p.name.Contains("Lid"))).ToArray())
                .Length == 0,
            "/tracking/eye/EyesClosedAmount"
        ),
        
        #endregion

        #region Eye Pupils
        
        new EParam(exp => exp.Eye.Combined().PupilDiameter_MM, "v2/PupilDilation"),

        new EParam(exp => exp.Eye.Left.PupilDiameter_MM * 0.1f, "v2/PupilDiameterLeft"),
        new EParam(exp => exp.Eye.Right.PupilDiameter_MM * 0.1f, "v2/PupilDiameterRight"),
        new EParam(exp => (exp.Eye.Left.PupilDiameter_MM * 0.1f + exp.Eye.Left.PupilDiameter_MM * 0.1f) / 2.0f, "v2/PupilDiameter"),
        
        #endregion

        #region Eye Openness
        
        new EParam(exp => exp.Eye.Left.Openness, "v2/EyeOpenLeft"),
        new EParam(exp => exp.Eye.Right.Openness, "v2/EyeOpenRight"),
        new EParam(exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f, "v2/EyeOpen"),

        #endregion

        #region Eyes Widen (Combined)

        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
            ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight
            : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight,
            "v2/EyeWide"),

        #endregion

        #region Eyelids Combined

        new EParam(exp => exp.Eye.Left.Openness * .75f + exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .25f, "v2/EyeLidLeft"),
        new EParam(exp => exp.Eye.Right.Openness * .75f + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .25f, "v2/EyeLidRight"),
        new EParam(exp =>
            ((exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f) * .75f +
            ((exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight) / 2.0f) * .25f,
            "v2/EyeLid"),
       
        #endregion

        #region Eyes Squint (Combined)

        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight
            ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight
            : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight,
            "v2/EyesSquint"),
        
        #endregion

        #region Eyebrows Compacted

        new EParam(exp => 
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight) + GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft),
            "v2/BrowDown"),

        new EParam(exp => 
            (exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight) / 2.0f,
            "v2/BrowInnerUp"),
        new EParam(exp => 
            (exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight) / 2.0f,
            "v2/BrowOuterUp"),

        // -1 = 'Angry', +1 = 'Worried'
        new EParam(exp => 
            Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight * .5f) -
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight),
            "v2/BrowExpressionRight"),

        new EParam(exp => 
            Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight * .5f) -
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft),
            "v2/BrowExpressionLeft"),

        new EParam(exp =>
            (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight * .5f) -
            Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight * .5f)) +
            GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownRight) * .5f - GetSimpleShape(exp, UnifiedSimpleExpressions.BrowDownLeft) * .5f,
            "v2/BrowExpression"),

        #endregion

        #region Jaw Combined

        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.JawRight].Weight - exp.Shapes[(int)UnifiedExpressions.JawLeft].Weight, "v2/JawX"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.JawForward].Weight - exp.Shapes[(int)UnifiedExpressions.JawBackward].Weight, "v2/JawZ"),

        #endregion

        #region Cheeks Combined

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2.0f, "v2/CheekSquint"),

        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight, "v2/CheekPuffSuckLeft"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight, "v2/CheekPuffSuckRight"),

        new EParam(exp =>
            (exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight + exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight) / 2.0f,
            "v2/CheekPuffSuck"),

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2.0f, "v2/CheekSuck"),

        #endregion

        #region Mouth Direction

        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].Weight, "v2/MouthUpperX"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].Weight, "v2/MouthLowerX"),

        new EParam(exp =>
            (exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].Weight) / 2.0f -
            (exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].Weight) / 2.0f,
            "v2/MouthX"),

        #endregion

        #region Lip Combined

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight) / 2.0f, "v2/LipSuckUpper"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight) / 2.0f, "v2/LipSuckLower"),
        new EParam(exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight +
            exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight) / 4.0f, 
            "v2/LipSuck"),

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight) / 2.0f, "v2/LipFunnelUpper"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 2.0f, "v2/LipFunnelLower"),
        new EParam(exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight + 
            exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 4.0f, 
            "v2/LipFunnel"),

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight) / 2.0f, "v2/LipPuckerRight"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 2.0f, "v2/LipPuckerLeft"),
        new EParam(exp => 
            (exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight +
            exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight) / 4.0f, 
            "v2/LipPucker"),

        #endregion

        #region Mouth Combined
        
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * .5f, "v2/MouthUpperUp"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * .5f + exp.Shapes[(int) UnifiedExpressions.MouthLowerDownLeft].Weight * .5f, "v2/MouthLowerDown"),
        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight * .25f + exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight * .25f +
            exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight * .25f + exp.Shapes[(int) UnifiedExpressions.MouthLowerDownLeft].Weight * .25f,
            "v2/MouthOpen"),

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight) / 2.0f, "v2/MouthStretch"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight) / 2.0f, "v2/MouthTightener"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight) / 2.0f, "v2/MouthPress"),
        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight) / 2.0f, "v2/MouthDimple"),

        new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight + exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight) / 2.0f, "v2/NoseSneer"),

        #endregion

        #region Lip Corners Combined

        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight, 
            "v2/MouthCornerYLeft"),
        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight, 
            "v2/MouthCornerYRight"),
        new EParam(exp =>
            exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight * .5f -
            exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight, "v2/MouthCornerY"),

        new EParam(exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) - 
            exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight, "v2/SmileFrownRight"),
        new EParam(exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) - 
            exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight, "v2/SmileFrownLeft"),

        new EParam(exp =>
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) * .5f + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) * .5f -
            exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight * .5f - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight * .5f,
            "v2/SmileFrown"),

        // Smile 'Sad' contains both the stretcher and frown shapes to represent sad (similar in functionality to SRanipal Sad, just with explicit acknowledgment of lessened tracking fidelity).
        new EParam(exp => 
            GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight) - GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadRight),
            "v2/SmileSadRight"),

        new EParam(exp => 
        GetSimpleShape(exp,
            UnifiedSimpleExpressions.MouthSmileLeft) - GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadLeft),
            "v2/SmileSadLeft"),

        new EParam(exp =>
            (GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileLeft) + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSmileRight)) / 2.0f -
            (GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadLeft) + GetSimpleShape(exp, UnifiedSimpleExpressions.MouthSadRight)) / 2.0f,
            "v2/SmileSad"),

        #endregion 

        #region Tongue Combined

        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].Weight - exp.Shapes[(int)UnifiedExpressions.TongueLeft].Weight, "v2/TongueX"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].Weight - exp.Shapes[(int)UnifiedExpressions.TongueDown].Weight, "v2/TongueY"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueCurlUp].Weight - exp.Shapes[(int)UnifiedExpressions.TongueBendDown].Weight, "v2/TongueArchY"),
        new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueFlat].Weight - exp.Shapes[(int)UnifiedExpressions.TongueSquish].Weight, "v2/TongueShape"),

        #endregion

        new ConditionalBoolParameter(exp => (UnifiedLibManager.EyeStatus == ModuleState.Active, UnifiedLibManager.EyeStatus != ModuleState.Uninitialized), "EyeTrackingActive"),
        new ConditionalBoolParameter(exp => (UnifiedLibManager.ExpressionStatus == ModuleState.Active, UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized), "ExpressionTrackingActive"),
        new ConditionalBoolParameter(exp => (UnifiedLibManager.ExpressionStatus == ModuleState.Active, UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized), "LipTrackingActive")

    };

    public static readonly IParameter[] ExpressionParameters =
        GetAllBaseExpressions().Union(GetAllBaseSimpleExpressions()).Union(UnifiedCombinedShapes).ToArray();

    private static IEnumerable<EParam> GetAllBaseExpressions() =>
        ((UnifiedExpressions[])Enum.GetValues(typeof(UnifiedExpressions))).ToList().Select(shape =>
           new EParam(exp => exp.Shapes[(int)shape].Weight,
               "v2/" + shape.ToString(), 0.0f));
    private static IEnumerable<EParam> GetAllBaseSimpleExpressions() =>
        ((UnifiedSimpleExpressions[])Enum.GetValues(typeof(UnifiedSimpleExpressions))).ToList().Select(simple =>
           new EParam(exp => GetSimpleShape(exp, simple),
               "v2/" + simple.ToString(), 0.0f));

    private static float GetSimpleShape(UnifiedTrackingData data, UnifiedSimpleExpressions expression) => UnifiedSimplifier.ExpressionMap[expression].Invoke(data);
}
