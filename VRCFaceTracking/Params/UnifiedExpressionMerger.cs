using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Params
{
    public static class UnifiedExpressionMerger
    {
        public static readonly IParameter[] UnifiedCombinedShapes =
        {    
            // Eye Definitions
            
            #region Eye Gaze
            
            new XYParameter(exp => exp.ReadInternal().Eye.Combined.GazeNormalized, "GazeX", "GazeY"),
            new XYParameter(exp => exp.ReadInternal().Eye.Left.GazeNormalized, "LeftGazeX", "LeftGazeY"),
            new XYParameter(exp => exp.ReadInternal().Eye.Right.GazeNormalized, "RightGazeX", "RightGazeY"),
            
            #endregion

            #region Eye Openness
            
            new EParam(exp => exp.ReadInternal().Eye.Left.Openness, "LeftEyeOpen"),
            new EParam(exp => exp.ReadInternal().Eye.Right.Openness, "RightEyeOpen"),
            new EParam(exp => (exp.ReadInternal().Eye.Left.Openness + exp.ReadInternal().Eye.Right.Openness) / 2.0f, "EyeOpen"),

            #endregion
            
            #region Eye Widen

            new EParam(exp =>
                exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] 
                ? exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] 
                : exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight],
                "EyesWiden"),
            
            #endregion

            #region Eye Openess Expanded

            new EParam(exp => (exp.ReadInternal().Eye.Left.Openness * 0.5f) + (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] * 0.5f), "LeftEyeOpenExpanded"),
            new EParam(exp => (exp.ReadInternal().Eye.Right.Openness * 0.5f) + (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] * 0.5f), "RightEyeOpenExpanded"),

            new EParam(exp => 
            (exp.ReadInternal().Eye.Right.Openness * 0.5f) + (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] * 0.5f) + (exp.ReadInternal().Eye.Left.Openness * 0.5f) + (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] * 0.5f) / 2.0f
            , "EyeOpenExpanded"),
            
            #endregion

            // Combined Expressions

            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.BrowInnerUpLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.BrowInnerUpRight], "TestInnerBrow"),

            #region Status

            new BoolParameter(exp => UnifiedLibManager.EyeStatus.Equals(ModuleState.Active), "EyeTrackingActive"),
            new BoolParameter(exp => UnifiedLibManager.ExpressionStatus.Equals(ModuleState.Active), "LipTrackingActive"),

            #endregion

        };

        public static readonly IParameter[] ExpressionParameters =
            GetAllBaseExpressions().ToArray();

        private static IEnumerable<EParam> GetAllBaseExpressions() =>
            ((UnifiedExpressions[])Enum.GetValues(typeof(UnifiedExpressions))).ToList().Select(shape =>
               new EParam(exp => exp.ReadInternal().Shapes[(int)shape],
                   shape.ToString(), 0.0f));

        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}
