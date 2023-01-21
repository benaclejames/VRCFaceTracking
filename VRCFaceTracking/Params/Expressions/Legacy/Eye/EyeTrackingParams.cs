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

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight, "EyesWiden"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight, "LeftEyeWiden"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight, "RightEyeWiden"),
            
            #endregion
            
            #region Squeeze
            
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight, "EyesSqueeze"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight, "LeftEyeSqueeze"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight, "RightEyeSqueeze"),
            
            #endregion
            
            #region Dilation
            
            new EParam(exp => exp.Eye.Combined().PupilDiameter_MM, "EyesDilation"),
            new EParam(exp => (exp.Eye.Left.PupilDiameter_MM + exp.Eye.Left.PupilDiameter_MM) / 2.0f, "EyesPupilDiameter"),
            
            #endregion
            
            #region EyeLid
            
            new EParam(exp => exp.Eye.Left.Openness, "LeftEyeLid"),
            new EParam(exp => exp.Eye.Right.Openness, "RightEyeLid"),
            new EParam(exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness)/2, "CombinedEyeLid"),
            
            #endregion
            
            #region EyeLidExpanded
            
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight * 0.2f + exp.Eye.Left.Openness * 0.8f, "LeftEyeLidExpanded", 0.5f, true),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight * 0.2f + exp.Eye.Right.Openness * 0.8f, "RightEyeLidExpanded", 0.5f, true),
            new EParam(exp => ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight) / 2.0f) * 0.2f + 
                ((exp.Eye.Right.Openness + exp.Eye.Right.Openness) / 2.0f) * 0.8f, "EyeLidExpanded", 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight * 0.2f + exp.Eye.Left.Openness * 0.8f - exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight, "LeftEyeLidExpandedSqueeze", 0.5f, true),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight * 0.2f + exp.Eye.Right.Openness * 0.8f - exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight, "RightEyeLidExpandedSqueeze", 0.5f, true),
            new EParam(exp => ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight) / 2.0f) * 0.2f + 
                ((exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f) * 0.8f -
                ((exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight) / 2.0f), 
                "EyeLidExpandedSqueeze", 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight) / 2.0f > 0)
                    return (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight) / 2.0f;
                return exp.Eye.Combined().Openness;
            }, "CombinedEyeLidExpanded"),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight / 2.0f > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight / 2.0f; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight / 2.0f > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight / 2.0f;
                return exp.Eye.Combined().Openness;
            }, "CombinedEyeLidExpandedSqueeze"),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > 0, "LeftEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight > 0, "RightEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].AdjustedWeight > 0
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].AdjustedWeight > 0
                , "EyesWidenToggle"),

            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].AdjustedWeight > 0
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].AdjustedWeight > 0
                , "EyesSquintToggle"),

            #endregion
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}