using System;

namespace VRCFaceTracking.Params.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly IParameter[] ParameterList = {
            #region XYParams
            
            new EParam(exp => exp.Eye.Combined().Gaze, "Eyes"),
            new EParam(exp => exp.Eye.Left.Gaze, "LeftEye"),
            new EParam(exp => exp.Eye.Right.Gaze, "RightEye"),
            
            #endregion
            
            #region Widen

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight, "LeftEyeWiden"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight, "RightEyeWiden"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f, "EyeWiden"),
           
            #endregion
            
            #region Squeeze
            
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight, "LeftEyeSqueeze"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight, "RightEyeSqueeze"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f, "EyesSqueeze"),
            
            #endregion
            
            #region Dilation
            
            new EParam(exp => exp.Eye.Combined().PupilDiameter_MM, "EyesDilation"),
            new EParam(exp => (exp.Eye.Left.PupilDiameter_MM + exp.Eye.Left.PupilDiameter_MM) / 2.0f, "EyesPupilDiameter"),
            
            #endregion
            
            #region EyeLid
            
            new EParam(exp => exp.Eye.Left.Openness, "LeftEyeLid"),
            new EParam(exp => exp.Eye.Right.Openness, "RightEyeLid"),
            new EParam(exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f, "CombinedEyeLid"),
            
            #endregion
            
            #region EyeLidExpanded
            
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * 0.2f + exp.Eye.Left.Openness * 0.8f, "LeftEyeLidExpanded", 0.5f, true),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * 0.2f + exp.Eye.Right.Openness * 0.8f, "RightEyeLidExpanded", 0.5f, true),
            new EParam(exp => ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f) * 0.2f +
                ((exp.Eye.Right.Openness + exp.Eye.Right.Openness) / 2.0f) * 0.8f, "EyeLidExpanded", 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + 
                0.8f * exp.Eye.Left.Openness - exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight,
                "LeftEyeLidExpandedSqueeze", 0.5f, true),
            new EParam(exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f +
                0.8f * exp.Eye.Right.Openness - exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight,
                "RightEyeLidExpandedSqueeze", 0.5f, true),
            new EParam(exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f +
                0.8f *exp.Eye.Left.Openness - exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight) + 
                (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f +
                0.8f * exp.Eye.Right.Openness - exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f, 
                "EyeLidExpandedSqueeze", 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > (1.0f - exp.Eye.Left.Openness))
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > (1.0f - exp.Eye.Right.Openness))
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + 
                    exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight ) / 2.0f > ((1.0f - exp.Eye.Left.Openness) + (1.0f - exp.Eye.Right.Openness)) / 2.0f )
                    return (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f;
                return (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f ;
            }, "CombinedEyeLidExpanded"),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > (1.0f - exp.Eye.Left.Openness))
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > exp.Eye.Left.Openness)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > (1.0f - exp.Eye.Right.Openness))
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > exp.Eye.Right.Openness)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f > ((1.0f - exp.Eye.Left.Openness) + (1.0f - exp.Eye.Right.Openness)) / 2.0f)
                    return (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f; 
                if ((exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight) / 2.0f > (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f)
                    return (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight) / 2.0f;
                return exp.Eye.Combined().Openness;
            }, "CombinedEyeLidExpandedSqueeze"),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > (1.0f - exp.Eye.Left.Openness), "LeftEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > (1.0f - exp.Eye.Right.Openness), "RightEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > (1.0f - exp.Eye.Left.Openness)
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > (1.0f - exp.Eye.Right.Openness)
                , "EyesWidenToggle"),

            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > exp.Eye.Left.Openness, "LeftEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > exp.Eye.Right.Openness, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > exp.Shapes[(int) UnifiedExpressions.EyeSquintRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > exp.Eye.Left.Openness
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > exp.Eye.Right.Openness
                , "EyesSqueezeToggle"),

            #endregion

            #region Quest Pro Legacy

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight 
                > exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight 
                ? exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight 
                : exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight, 
                "BrowsInnerUp"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight, "BrowInnerUpLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight, "BrowInnerUpRight"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight 
                > exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight 
                ? exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight, 
                "BrowsOuterUp"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight, "BrowOuterUpLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight, "BrowOuterUpRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight) / 2.0f
                > (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight) / 2.0f
                ? (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight) / 2.0f
                : (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight) / 2.0f, 
                "BrowsDown"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft].Weight) / 2.0f, "BrowDownLeft"),
            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.BrowInnerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowOuterDownRight].Weight) / 2.0f, "BrowDownRight"),

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserLower].Weight, "MouthRaiserLower"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserUpper].Weight, "MouthRaiserUpper"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f, "EyesSquint"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight, "EyeSquintLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight, "EyeSquintRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2.0f, "CheeksSquint"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight, "CheekSquintLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight, "CheekSquintRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight) / 2.0f, "MouthDimple"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight, "MouthDimpleLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight, "MouthDimpleRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight) / 2.0f, "MouthPress"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight, "MouthPressLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight, "MouthPressRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight) / 2.0f, "MouthStretch"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight, "MouthStretchLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight, "MouthStretchRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight) / 2.0f, "MouthTightener"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight, "MouthTightenerLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight, "MouthTightenerRight"),

            new EParam(exp => (exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight + exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight) / 2.0f, "NoseSneer"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight, "NoseSneerLeft"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight, "NoseSneerRight"),

            #endregion
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}