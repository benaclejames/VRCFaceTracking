namespace VRCFaceTracking.Params.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly IParameter[] ParameterList = {
            #region XYParams
            
            new XYParameter(exp => exp.ReadInternal().Eye.Combined.GazeNormalized, "EyesX", "EyesY"),
            new XYParameter(exp => exp.ReadInternal().Eye.Left.GazeNormalized, "LeftEyeX", "LeftEyeY"),
            new XYParameter(exp => exp.ReadInternal().Eye.Right.GazeNormalized, "RightEyeX", "RightEyeY"),
            
            #endregion
            
            #region Widen

            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] 
                ? exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] 
                : exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight], "EyesWiden"),
            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft], "LeftEyeWiden"),
            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight], "RightEyeWiden"),
            
            #endregion
            
            #region Squeeze
            
            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight], "EyesSqueeze"),
            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft], "LeftEyeSqueeze"),
            new EParam(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight], "RightEyeSqueeze"),
            
            #endregion
            
            #region Dilation
            
            new EParam(exp => exp.ReadInternal().Eye.Left.PupilDiameter_MM, "EyesDilation"),
            new EParam(exp => exp.ReadInternal().Eye.Left.PupilDiameter_MM, "EyesPupilDiameter"),
            
            #endregion
            
            #region EyeLid
            
            new EParam(exp => exp.ReadInternal().Eye.Left.Openness, "LeftEyeLid"),
            new EParam(exp => exp.ReadInternal().Eye.Right.Openness, "RightEyeLid"),
            new EParam(exp => (exp.ReadInternal().Eye.Left.Openness + exp.ReadInternal().Eye.Right.Openness)/2, "CombinedEyeLid"),
            
            #endregion
            
            #region EyeLidExpanded
            
            new EParam(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft]);
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Left.Openness);
            }, "LeftEyeLidExpanded", 0.5f, true),

            new EParam(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]); 
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Right.Openness);
            }, "RightEyeLidExpanded", 0.5f, true),

            new EParam(exp =>
            {
                if ((exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f); 
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Combined.Openness);
            }, "CombinedEyeLidExpanded", 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft]); 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] * -1;     
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Left.Openness);
            } ,"LeftEyeLidExpandedSqueeze", 0.5f, true),

            new EParam(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]); 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] * -1;        
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Right.Openness);
            } ,"RightEyeLidExpandedSqueeze", 0.5f, true),

            new EParam(exp =>
            {
                if ((exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f); 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] * -1;      
                return NormalizeFloat(0, 1, 0, 0.8f, exp.ReadInternal().Eye.Combined.Openness);
            } ,"CombinedEyeLidExpandedSqueeze", 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft];
                return exp.ReadInternal().Eye.Left.Openness;
            }, "LeftEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight];
                return exp.ReadInternal().Eye.Right.Openness;
            }, "RightEyeLidExpanded"),

            new BinaryParameter(exp =>
            {
                if ((exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f > 0)
                    return (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]) / 2.0f;
                return exp.ReadInternal().Eye.Combined.Openness;
            }, "CombinedEyeLidExpanded"),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            new BinaryParameter(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft]; 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft];
                return exp.ReadInternal().Eye.Left.Openness;
            }, "LeftEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight]; 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight];
                return exp.ReadInternal().Eye.Right.Openness;
            }, "RightEyeLidExpandedSqueeze"),
            
            new BinaryParameter(exp =>
            {
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] / 2.0f > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] / 2.0f; 
                if (exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] / 2.0f > 0)
                    return exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] + exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] / 2.0f;
                return exp.ReadInternal().Eye.Combined.Openness;
            }, "CombinedEyeLidExpandedSqueeze"),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0, "LeftEyeWidenToggle"),
            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0, "RightEyeWidenToggle"),
            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] 
                ? exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideLeft] > 0
                : exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeWideRight] > 0
                , "EyesWidenToggle"),

            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(exp => exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] > exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight]
                ? exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintLeft] > 0
                : exp.ReadInternal().Shapes[(int)UnifiedExpressions.EyeSquintRight] > 0
                , "EyesSquintToggle"),

            #endregion
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}