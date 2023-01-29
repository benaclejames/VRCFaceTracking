using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Params
{
    public static class UnifiedExpressionsMerger
    {
        public static readonly IParameter[] UnifiedCombinedShapes =
        {    
            // Unified Eye Definitions
            
            #region Eye Gaze
            
            new EParam(exp => exp.Eye.Combined().Gaze, "v2/Eyes"),
            new EParam(exp => exp.Eye.Left.Gaze, "v2/LeftEye"),
            new EParam(exp => exp.Eye.Right.Gaze, "v2/RightEye"),
            
            #endregion

            #region Eye Openness
            
            new EParam(exp => exp.Eye.Left.Openness, "v2/EyeOpenLeft"),
            new EParam(exp => exp.Eye.Right.Openness, "v2/EyeOpenRight"),
            new EParam(exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f, "v2/EyesOpen"),

            #endregion

            #region Eyes Widen (Combined)

            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight,
                "v2/EyesWiden"),
            
            #endregion

            #region Eyes Squint (Combined)

            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight  > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight,
                "v2/EyesSquint"),
            
            #endregion

            #region Eye Expressions

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight - exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight, "v2/LeftEyeExpression"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight - exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight, "v2/RightEyeExpression"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight) / 2.0f -
                (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f,
                "v2/EyesExpression"),

            
            #endregion

            #region Eyebrows Compacted

            new EParam(exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight),
                "v2/BrowYRight"),

            new EParam(exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight),
                "v2/BrowYLeft"),

            new EParam(exp =>
                (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight)) +
                (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight)) / 2.0f,
                "v2/BrowsY"),

            // Kinda complicated, basically reports the 'angle' of the brow normalized from -1.0 - 1.0, with -1.0 being angry and 1 being worried. Can be used in tandem with the
            // BrowY parameters to regain brow positioning but can be used by itself to control a simplified expression of the brows.
            new EParam(exp =>
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight, 2.0f),
                "v2/BrowExpressionRight"),

            new EParam(exp =>
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight, 2.0f),
                "v2/BrowExpressionLeft"),

            new EParam(exp =>
                ((float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight, 2.0f)) +
                ((float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight, 2.0f)) / 2.0f,
                "v2/BrowsExpression"),

            #endregion

            #region Jaw Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.JawRight].Weight - exp.Shapes[(int)UnifiedExpressions.JawLeft].Weight, "v2/JawX"),

            #endregion

            #region Mouth Closed Combined

            #endregion

            #region Cheeks Combined

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2.0f, "v2/CheekSquint"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight, "v2/CheekLeftPuffSuck"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight - exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight, "v2/CheekRightPuffSuck"),

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


            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight) / 2.0f, "v2/LipFunnelUpper"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight) / 2.0f, "v2/LipFunnelLower"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerLeft].Weight) / 2.0f, "v2/LipPucker"),

            #endregion

            #region Mouth Combined

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight) / 2.0f, "v2/MouthUpperUp"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight) / 2.0f, "v2/MouthLowerDown"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight) / 2.0f, "v2/MouthStretch"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight) / 2.0f, "v2/MouthTightener"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight) / 2.0f, "v2/MouthPress"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight) / 2.0f, "v2/MouthDimple"),

            #endregion

            #region Smile Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight, "v2/SmileFrownRight"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].Weight - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight, "v2/SmileFrownLeft"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].Weight) / 2.0f -
                (exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight) / 2.0f,
                "v2/SmileFrown"),

            // Smile 'Sad' contains both the stretcher and frown shapes to represent sad (similar in functionality to SRanipal Sad, just with explicit acknowledgment of more tracking fidelity).
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].Weight -
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight),
                "v2/SmileSadRight"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].Weight -
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight),
                "v2/SmileSadLeft"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].Weight) / 2.0f -
                (Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight) +
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight)) / 2.0f,
                "v2/SmileSad"),

            #endregion 

            #region Tongue Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].Weight - exp.Shapes[(int)UnifiedExpressions.TongueLeft].Weight, "v2/TongueX"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].Weight - exp.Shapes[(int)UnifiedExpressions.TongueDown].Weight, "v2/TongueY"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueCurl].Weight - exp.Shapes[(int)UnifiedExpressions.TongueBend].Weight, "v2/TongueArch"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueFlat].Weight - exp.Shapes[(int)UnifiedExpressions.TongueSquish].Weight, "v2/TongueShape"),

            #endregion

            #region Status

            new BoolParameter(exp => UnifiedLibManager.EyeStatus.Equals(ModuleState.Active), "EyeTrackingActive"),
            new BoolParameter(exp => UnifiedLibManager.ExpressionStatus.Equals(ModuleState.Active), "LipTrackingActive"),

            #endregion

        };

        public static readonly IParameter[] ExpressionParameters =
            GetAllBaseExpressions().Union(UnifiedCombinedShapes).ToArray();

        private static IEnumerable<EParam> GetAllBaseExpressions() =>
            ((UnifiedExpressions[])Enum.GetValues(typeof(UnifiedExpressions))).ToList().Select(shape =>
               new EParam(exp => exp.Shapes[(int)shape].Weight,
                   "v2/" + shape.ToString(), 0.0f));
    }
}
