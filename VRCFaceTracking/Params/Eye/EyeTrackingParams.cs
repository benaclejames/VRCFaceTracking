namespace VRCFaceTracking.Params.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly IParameter[] ParameterList = {
            #region XYParams
            
            new XYParameter(exp => exp.Eye.Combined().Gaze, "EyesX", "EyesY"),
            new XYParameter(exp => exp.Eye.Left.Gaze, "LeftEyeX", "LeftEyeY"),
            new XYParameter(exp => exp.Eye.Right.Gaze, "RightEyeX", "RightEyeY"),
            
            #endregion
            
            #region Widen

            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight, "EyesWiden"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight, "LeftEyeWiden"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight, "RightEyeWiden"),
            
            #endregion
            
            #region Squeeze
            
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight, "EyesSqueeze"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight, "LeftEyeSqueeze"),
            new EParam(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight, "RightEyeSqueeze"),
            
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
            
            new EParam(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight);
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Left.Openness);
            }, "LeftEyeLidExpanded", 0.5f, true),

            new EParam(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight); 
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Right.Openness);
            }, "RightEyeLidExpanded", 0.5f, true),

            new EParam(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f); 
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Combined().Openness);
            }, "CombinedEyeLidExpanded", 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight); 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight * -1;     
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Left.Openness);
            } ,"LeftEyeLidExpandedSqueeze", 0.5f, true),

            new EParam(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight); 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight * -1;        
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Right.Openness);
            } ,"RightEyeLidExpandedSqueeze", 0.5f, true),

            new EParam(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f); 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight * -1;      
                return NormalizeFloat(0, 1, 0, 0.8f, exp.Eye.Combined().Openness);
            } ,"CombinedEyeLidExpandedSqueeze", 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f > 0)
                    return (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f;
                return exp.Eye.Combined().Openness;
            }, "CombinedEyeLidExpanded"),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight;
                return exp.Eye.Left.Openness;
            }, "LeftEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight;
                return exp.Eye.Right.Openness;
            }, "RightEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight / 2.0f > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight / 2.0f; 
                if (exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight / 2.0f > 0)
                    return exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight / 2.0f;
                return exp.Eye.Combined().Openness;
            }, "CombinedEyeLidExpandedSqueeze"),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0, "LeftEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0, "RightEyeWidenToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight > 0
                : exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight > 0
                , "EyesWidenToggle"),

            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight
                ? exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight > 0
                : exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight > 0
                , "EyesSquintToggle"),

            #endregion
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}