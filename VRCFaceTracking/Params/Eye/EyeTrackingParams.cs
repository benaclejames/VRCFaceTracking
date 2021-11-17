using System.Collections.Generic;

namespace VRCFaceTracking.Params.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly List<IParameter> ParameterList = new List<IParameter>
        {
            new XYParameter(v2 => v2.Combined.Look, "EyesX", "EyesY"),

            new FloatEyeParameter(v2 => v2.Left.Widen > v2.Right.Widen ? v2.Left.Widen : v2.Right.Widen, "EyesWiden"),

            new FloatEyeParameter(v2 => v2.EyesDilation, "EyesDilation"),

            new XYParameter(v2 => v2.Left.Look, "LeftEyeX", "LeftEyeY"),
            new XYParameter(v2 => v2.Right.Look, "RightEyeX", "RightEyeY"),

            new FloatEyeParameter(v2 => v2.Left.Openness, "LeftEyeLid", true),
            new FloatEyeParameter(v2 => v2.Right.Openness, "RightEyeLid", true),
            new FloatEyeParameter(v2 => (v2.Left.Openness + v2.Right.Openness)/2, "CombinedEyeLid", true),

            new BoolEyeParameter(v2 => v2.Left.Openness < 0.5f, "LeftEyeLid"),
            new BoolEyeParameter(v2 => v2.Right.Openness < 0.5f, "RightEyeLid"),
            new BoolEyeParameter(v2 => (v2.Left.Openness + v2.Right.Openness)/2 < 0.5f, "CombinedEyeLid"),

            // BinaryEyeParameter takes in a single parameter of String, and creates
            // 4 bool VRC parameters as String1, String2, String4, and String8
            // represented in the binary counting system.
            new BinaryEyeParameter(v2 => v2.Left.Openness, "LeftEyeLid"),
            new BinaryEyeParameter(v2 => v2.Right.Openness, "RightEyeLid"),
            new BinaryEyeParameter(v2 => (v2.Left.Openness + v2.Right.Openness)/2, "CombinedEyeLid"),

            new BinaryEyeParameter(v2 =>
            {
                if (v2.Left.Widen > 0)
                    return v2.Left.Widen;
                return v2.Left.Openness;
            } ,"LeftEyeLidExpanded"),
            
            new BinaryEyeParameter(v2 =>
            {
                if (v2.Right.Widen > 0)
                    return v2.Right.Widen;
                return v2.Right.Openness;
            } ,"RightEyeLidExpanded"),

            new BinaryEyeParameter(v2 =>
            {
                if ((v2.Left.Widen + v2.Right.Widen) / 2 > 0)
                    return (v2.Left.Widen + v2.Right.Widen) / 2;
                return (v2.Left.Openness + v2.Right.Openness)/2;
            } ,"CombinedEyeLidExpanded"),

            new BinaryEyeParameter(v2 =>
            {
                if (v2.Left.Widen > 0)
                    return v2.Left.Widen;
                if (v2.Left.Squeeze > 0)
                    return v2.Left.Squeeze;
                return v2.Left.Openness;
            } ,"LeftEyeLidExpandedSqueeze"),

            new BinaryEyeParameter(v2 =>
            {
                if (v2.Right.Widen > 0)
                    return v2.Right.Widen;
                if (v2.Right.Squeeze > 0)
                    return v2.Right.Squeeze;
                return v2.Right.Openness;
            } ,"RightEyeLidExpandedSqueeze"),

            new BinaryEyeParameter(v2 =>
            {
                if ((v2.Left.Widen + v2.Right.Widen) / 2 > 0)
                    return (v2.Left.Widen + v2.Right.Widen) / 2;
                if ((v2.Left.Squeeze + v2.Right.Squeeze) / 2 > 0)
                    return (v2.Left.Squeeze + v2.Right.Squeeze) / 2;
                return (v2.Left.Openness + v2.Right.Openness) / 2;
            } ,"CombinedEyeLidExpandedSqueeze"),

            // Use these in combination with the binary params above to help with animation states
            // of the Expanded (Widen & blink only) or ExpandedSqueeze (Widen, Squeeze, Blink) eyelids
            new BoolEyeParameter(v2 => v2.Left.Widen > 0, "LeftEyeWidenToggle"),
            new BoolEyeParameter(v2 => v2.Right.Widen > 0, "RightEyeWidenToggle"),
            new BoolEyeParameter(v2 => v2.Combined.Widen > 0, "EyesWidenToggle"),

            new FloatEyeParameter(v2 => v2.Left.Squeeze, "LeftEyeSqueeze"),
            new FloatEyeParameter(v2 => v2.Right.Squeeze, "RightEyeSqueeze"),

            new BoolEyeParameter(v2 => v2.Left.Squeeze > 0, "LeftEyeSqueezeToggle"),
            new BoolEyeParameter(v2 => v2.Right.Squeeze > 0, "RightEyeSqueezeToggle"),
            new BoolEyeParameter(v2 => v2.Combined.Squeeze > 0, "EyesSqueezeToggle"),

            new FloatEyeParameter(v2 =>
            {
                if (v2.Left.Openness >= 1 && v2.Left.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Left.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Left.Openness);
            }, "LeftEyeLidExpanded", true),
            new FloatEyeParameter(v2 =>
            {
                if (v2.Right.Openness >= 1 && v2.Right.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Right.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Right.Openness);
            }, "RightEyeLidExpanded", true),

            new FloatEyeParameter(v2 => v2.Left.Widen, "LeftEyeWiden"),
            new FloatEyeParameter(v2 => v2.Right.Widen, "RightEyeWiden"),

            new FloatEyeParameter(v2 => v2.Left.Squeeze, "LeftEyeSqueeze"),
            new FloatEyeParameter(v2 => v2.Right.Squeeze, "RightEyeSqueeze"),
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}