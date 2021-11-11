using System.Collections.Generic;

namespace VRCFaceTracking.Params
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

            // Testing out the bool idea: Ideally would handle this in a class, but a test is a test.
            new BoolEyeParameter(v2 => ((int)(((v2.Left.Openness * 15 + .5))%2) == 1), "LeftEyeLid1"),
            new BoolEyeParameter(v2 => ((int)(((v2.Left.Openness * 15 + .5)/2)%2) == 1), "LeftEyeLid2"),
            new BoolEyeParameter(v2 => ((int)(((v2.Left.Openness * 15 + .5)/4)%2) == 1), "LeftEyeLid4"),
            new BoolEyeParameter(v2 => ((int)(((v2.Left.Openness * 15 + .5)/8)%2) == 1), "LeftEyeLid8"),

            new BoolEyeParameter(v2 => (v2.Left.Widen == 0) & ((int)(((v2.Left.Openness * 15 + .5))%2) == 1) || ((int)(((v2.Left.Widen * 15 + .5))%2) == 1), "LeftEyeLidExpanded1"),
            new BoolEyeParameter(v2 => (v2.Left.Widen == 0) & ((int)(((v2.Left.Openness * 15 + .5)/2)%2) == 1) || ((int)(((v2.Left.Widen * 15 + .5)/2)%2) == 1), "LeftEyeLidExpanded2"),
            new BoolEyeParameter(v2 => (v2.Left.Widen == 0) & ((int)(((v2.Left.Openness * 15 + .5)/4)%2) == 1) || ((int)(((v2.Left.Widen * 15 + .5)/4)%2) == 1), "LeftEyeLidExpanded4"),
            new BoolEyeParameter(v2 => (v2.Left.Widen == 0) & ((int)(((v2.Left.Openness * 15 + .5)/8)%2) == 1) || ((int)(((v2.Left.Widen * 15 + .5)/8)%2) == 1), "LeftEyeLidExpanded8"),

            new BoolEyeParameter(v2 => ((int)(((v2.Right.Openness * 15 + .5))%2) == 1), "RightEyeLid1"),
            new BoolEyeParameter(v2 => ((int)(((v2.Right.Openness * 15 + .5)/2)%2) == 1), "RightEyeLid2"),
            new BoolEyeParameter(v2 => ((int)(((v2.Right.Openness * 15 + .5)/4)%2) == 1), "RightEyeLid4"),
            new BoolEyeParameter(v2 => ((int)(((v2.Right.Openness * 15 + .5)/8)%2) == 1), "RightEyeLid8"),

            new BoolEyeParameter(v2 => (v2.Right.Widen == 0) & ((int)(((v2.Right.Openness * 15 + .5))%2) == 1) || ((int)(((v2.Right.Widen * 15 + .5))%2) == 1), "RightEyeLidExpanded1"),
            new BoolEyeParameter(v2 => (v2.Right.Widen == 0) & ((int)(((v2.Right.Openness * 15 + .5)/2)%2) == 1) || ((int)(((v2.Right.Widen * 15 + .5)/2)%2) == 1), "RightEyeLidExpanded2"),
            new BoolEyeParameter(v2 => (v2.Right.Widen == 0) & ((int)(((v2.Right.Openness * 15 + .5)/4)%2) == 1) || ((int)(((v2.Right.Widen * 15 + .5)/4)%2) == 1), "RightEyeLidExpanded4"),
            new BoolEyeParameter(v2 => (v2.Right.Widen == 0) & ((int)(((v2.Right.Openness * 15 + .5)/8)%2) == 1) || ((int)(((v2.Right.Widen * 15 + .5)/8)%2) == 1), "RightEyeLidExpanded8"),

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