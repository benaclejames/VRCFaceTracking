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
            
            new EParam(exp => exp.AdjustedEye.Combined().Gaze, "v2/Eyes"),
            new EParam(exp => exp.AdjustedEye.Left.Gaze, "v2/LeftEye"),
            new EParam(exp => exp.AdjustedEye.Right.Gaze, "v2/RightEye"),
            
            #endregion

            #region Eye Openness
            
            new EParam(exp => exp.AdjustedEye.Left.Openness, "v2/LeftEyeOpen"),
            new EParam(exp => exp.AdjustedEye.Right.Openness, "v2/RightEyeOpen"),
            new EParam(exp => (exp.AdjustedEye.Left.Openness + exp.AdjustedEye.Right.Openness) / 2.0f, "v2/EyesOpen"),

            #endregion

            #region Eyes Widen (Combined)

            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight  > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight,
                "v2/EyesWiden"),
            
            #endregion

            #region Eyes Squint (Combined)

            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight  > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight,
                "v2/EyesSquint"),
            
            #endregion

            #region Eye Expressions

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight, "v2/LeftEyeExpression"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight, "v2/RightEyeExpression"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight) / 2.0f -
                (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight) / 2.0f,
                "v2/EyesExpression"),

            
            #endregion

            #region Eyebrows Compacted

            new EParam(exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].AdjustedWeight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].AdjustedWeight),
                "v2/RightBrowY"),

            new EParam(exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].AdjustedWeight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].AdjustedWeight),
                "v2/LeftBrowY"),

            new EParam(exp =>
                (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].AdjustedWeight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].AdjustedWeight)) +
                (Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].AdjustedWeight) -
                Math.Min(1, exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].AdjustedWeight)) / 2.0f,
                "v2/LeftBrowY"),

            // Kinda complicated, basically reports the 'angle' of the brow normalized from -1.0 - 1.0, with -1.0 being angry and 1 being worried. Can be used in tandem with the
            // BrowY parameters to regain brow positioning but can be used by itself to control a simplified expression of the brows.
            new EParam(exp =>
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].AdjustedWeight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].AdjustedWeight, 2.0f),
                "v2/RightBrowExpression"),

            new EParam(exp =>
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].AdjustedWeight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].AdjustedWeight, 2.0f),
                "v2/LeftBrowExpression"),

            new EParam(exp =>
                ((float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].AdjustedWeight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].AdjustedWeight, 2.0f)) +
                ((float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].AdjustedWeight, 2.0f) -
                (float)Math.Pow(exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].AdjustedWeight, 2.0f)) / 2.0f,
                "v2/BrowsExpression"),

            #endregion

            #region Jaw Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.JawRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.JawLeft].AdjustedWeight, "v2/JawX"),

            #endregion

            #region Mouth Closed Combined

            #endregion

            #region Cheeks Combined

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].AdjustedWeight) / 2.0f, "v2/CheekSquint"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].AdjustedWeight, "v2/CheekLeftPuffSuck"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].AdjustedWeight, "v2/CheekRightPuffSuck"),

            new EParam(exp => 
                (exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].AdjustedWeight) / 2.0f - 
                (exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].AdjustedWeight) / 2.0f, 
                "v2/CheekPuffSuck"),

            #endregion

            #region Mouth Direction

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].AdjustedWeight, "v2/MouthUpperX"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].AdjustedWeight, "v2/MouthLowerX"),

            new EParam(exp => 
                (exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].AdjustedWeight) / 2.0f -
                (exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].AdjustedWeight) / 2.0f, 
                "v2/MouthX"),

            #endregion

            #region Smile Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].AdjustedWeight, "v2/SmileFrownRight"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].AdjustedWeight, "v2/SmileFrownLeft"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].AdjustedWeight) / 2.0f -
                (exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].AdjustedWeight) / 2.0f,
                "v2/SmileFrown"),

            // Smile 'Sad' contains both the stretcher and frown shapes to represent sad (similar in functionality to SRanipal Sad, just with explicit acknowledgment of more tracking fidelity).
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].AdjustedWeight -
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].AdjustedWeight),
                "v2/SmileSadRight"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].AdjustedWeight -
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].AdjustedWeight),
                "v2/SmileSadLeft"),

            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].AdjustedWeight) / 2.0f -
                (Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].AdjustedWeight) +
                Math.Min(1.0f, exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].AdjustedWeight)) / 2.0f,
                "v2/SmileSad"),

            #endregion

            #region Misc Lip Combined

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].AdjustedWeight) / 2.0f, "v2/MouthStretch"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].AdjustedWeight) / 2.0f, "v2/MouthTightener"),

            #endregion

            #region Tongue Combined

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.TongueLeft].AdjustedWeight, "v2/TongueX"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.TongueDown].AdjustedWeight, "v2/TongueY"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueCurl].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.TongueBend].AdjustedWeight, "v2/TongueArch"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.TongueFlat].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.TongueSquish].AdjustedWeight, "v2/TongueShape"),

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
               new EParam(exp => exp.Shapes[(int)shape].AdjustedWeight,
                   "v2/" + shape.ToString(), 0.0f));
    }
}
