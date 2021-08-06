using System.Collections.Generic;
using UnityEngine;
using VRCFaceTracking.SRanipal;

namespace VRCFaceTracking.Params
{
    public static class EyeTrackingParams
    {
        public static readonly List<IParameter> ParameterList = new List<IParameter>
        {
            new XYEyeParameter(v2 => v2.Combined, "EyesX", "EyesY"),

            new FloatEyeParameter(v2 => v2.Left.Widen > v2.Right.Widen ? v2.Left.Widen : v2.Right.Widen, "EyesWiden"),

            new FloatEyeParameter(v2 =>
            {
                var normalizedFloat = v2.EyesDilation / SRanipalTrackingInterface.MinDilation /
                                      (SRanipalTrackingInterface.MaxDilation - SRanipalTrackingInterface.MinDilation);
                return Mathf.Clamp(normalizedFloat, 0, 1);
            }, "EyesDilation"),

            new XYEyeParameter(v2 => v2.Left, "LeftEyeX", "LeftEyeY"),
            new XYEyeParameter(v2 => v2.Right, "RightEyeX", "RightEyeY"),

            new FloatEyeParameter(v2 => v2.Left, "LeftEyeLid", true),
            new FloatEyeParameter(v2 => v2.Right, "RightEyeLid", true),
            new FloatEyeParameter(v2 => (v2.Left + v2.Right)/2, "CombinedEyeLid", true),
            
            new BoolEyeParameter(v2 => v2.Left < 0.5f, "LeftEyeLid"),
            new BoolEyeParameter(v2 => v2.Right < 0.5f, "RightEyeLid"),
            new BoolEyeParameter(v2 => (v2.Left + v2.Right)/2 < 0.5f, "CombinedEyeLid"),
            
            new FloatEyeParameter(v2 =>
            {
                if (v2.Left >= 1 && v2.Left.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Left.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Left);
            }, "LeftEyeLidExpanded", true),
            new FloatEyeParameter(v2 =>
            {
                if (v2.Right >= 1 && v2.Right.Widen > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.Right.Widen); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.Right);
            }, "RightEyeLidExpanded", true),

            new FloatEyeParameter(v2 => v2.Left.Widen, "LeftEyeWiden"),
            new FloatEyeParameter(v2 => v2.Right.Widen, "RightEyeWiden"),

            new FloatEyeParameter(v2 => v2.Left.Squeeze, "LeftEyeSqueeze"),
            new FloatEyeParameter(v2 => v2.Right.Squeeze, "RightEyeSqueeze")
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}
