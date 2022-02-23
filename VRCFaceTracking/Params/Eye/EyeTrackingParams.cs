using System.Collections.Generic;

namespace VRCFaceTracking.Params.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly List<IParameter> ParameterList = new List<IParameter>
        {
            #region XYParams
            
            new XYParameter(v2 => v2.Combined.Look, "EyesX", "EyesY"),
            new XYParameter(v2 => v2.Left.Look, "LeftEyeX", "LeftEyeY"),
            new XYParameter(v2 => v2.Right.Look, "RightEyeX", "RightEyeY"),
            
            #endregion
            
            #region Widen

            new EParam(v2 => v2.Left.Widen > v2.Right.Widen ? v2.Left.Widen : v2.Right.Widen, "EyesWiden"),
            new EParam(v2 => v2.Left.Widen, "LeftEyeWiden"),
            new EParam(v2 => v2.Right.Widen, "RightEyeWiden"),
            
            #endregion
            
            #region Squeeze
            
            new EParam(v2 => v2.Combined.Squeeze, "EyesSqueeze"),
            new EParam(v2 => v2.Left.Squeeze, "LeftEyeSqueeze"),
            new EParam(v2 => v2.Right.Squeeze, "RightEyeSqueeze"),
            
            #endregion
            
            #region Dilation
            
            new EParam(v2 => v2.EyesDilation, "EyesDilation"),
            
            #endregion
            
            #region EyeLid
            
            new EParam(v2 => v2.Left.Openness, "LeftEyeLid"),
            new EParam(v2 => v2.Right.Openness, "RightEyeLid"),
            new EParam(v2 => (v2.Left.Openness + v2.Right.Openness)/2, "CombinedEyeLid"),
            
            #endregion
            
            #region EyeLidExpanded
            
            new EParam((v2, eye) =>
            {
                if (v2.Left.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Left.Widen);
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Left.Openness);
            }, "LeftEyeLidExpanded", 0.5f, true),

            new EParam((v2, eye) =>
            {
                if (v2.Right.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Right.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Right.Openness);
            }, "RightEyeLidExpanded", 0.5f, true),

            new EParam((v2, eye) =>
            {
                if (v2.Combined.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Combined.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Combined.Openness);
            }, "CombinedEyeLidExpanded", 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam((v2, eye) =>
            {
                if (v2.Left.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Left.Widen); 
                if (v2.Left.Squeeze > 0)
                    return v2.Left.Squeeze * -1;     
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Left.Openness);
            } ,"LeftEyeLidExpandedSqueeze", 0.5f, true),

            new EParam((v2, eye) =>
            {
                if (v2.Right.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Right.Widen); 
                if (v2.Right.Squeeze > 0)
                    return v2.Right.Squeeze * -1;        
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Right.Openness);
            } ,"RightEyeLidExpandedSqueeze", 0.5f, true),

            new EParam((v2, eye) =>
            {
                if (v2.Combined.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Combined.Widen); 
                if (v2.Combined.Squeeze > 0)
                    return v2.Combined.Squeeze * -1;      
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Combined.Openness);
            } ,"CombinedEyeLidExpandedSqueeze", 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryParameter((v2, eye) =>
            {
                if (v2.Left.Widen > 0)
                    return v2.Left.Widen;
                return v2.Left.Openness;
            }, "LeftEyeLidExpanded"),

            new BinaryParameter((v2, eye) =>
            {
                if (v2.Right.Widen > 0)
                    return v2.Right.Widen;
                return v2.Right.Openness;
            }, "RightEyeLidExpanded"),

            new BinaryParameter((v2, eye) =>
            {
                if (v2.Combined.Widen > 0)
                    return v2.Combined.Widen;
                return v2.Combined.Openness;
            }, "CombinedEyeLidExpanded"),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            new BinaryParameter(v2 =>
            {
                if (v2.Left.Widen > 0)
                    return v2.Left.Widen; 
                if (v2.Left.Squeeze > 0)
                    return v2.Left.Squeeze;
                return v2.Left.Openness;
            }, "LeftEyeLidExpandedSqueeze"),
            
            new BinaryParameter(v2 =>
            {
                if (v2.Right.Widen > 0)
                    return v2.Right.Widen; 
                if (v2.Right.Squeeze > 0)
                    return v2.Right.Squeeze;
                return v2.Right.Openness;
            }, "RightEyeLidExpandedSqueeze"),
            
            new BinaryParameter((v2, eye) =>
            {
                if (v2.Combined.Widen > 0)
                    return v2.Combined.Widen; 
                if (v2.Combined.Squeeze > 0)
                    return v2.Combined.Squeeze;
                return v2.Combined.Openness;
            }, "CombinedEyeLidExpandedSqueeze"),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BoolParameter(v2 => v2.Left.Widen > 0, "LeftEyeWidenToggle"),
            new BoolParameter(v2 => v2.Right.Widen > 0, "RightEyeWidenToggle"),
            new BoolParameter(v2 => v2.Combined.Widen > 0, "EyesWidenToggle"),

            new BoolParameter(v2 => v2.Left.Squeeze > 0, "LeftEyeSqueezeToggle"),
            new BoolParameter(v2 => v2.Right.Squeeze > 0, "RightEyeSqueezeToggle"),
            new BoolParameter(v2 => v2.Combined.Squeeze > 0, "EyesSqueezeToggle"),

            #endregion
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}