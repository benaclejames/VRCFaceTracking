using System.Collections.Generic;
using UnityEngine;
using VRCFaceTracking.SRanipal;

namespace VRCFaceTracking.Params
{
    public static class EyeTrackingParams
    {
        public static readonly List<IParameter> ParameterList = new List<IParameter>
        {
            new XYEyeParameter(v2 => v2.Combined.Look, "EyesX", "EyesY"),

            new FloatEyeParameter(v2 => v2.Left.Widen > v2.Right.Widen ? v2.Left.Widen : v2.Right.Widen, "EyesWiden"),

            new FloatEyeParameter(v2 =>
            {
                var normalizedFloat = v2.EyesDilation / SRanipalTrackingInterface.MinDilation /
                                      (SRanipalTrackingInterface.MaxDilation - SRanipalTrackingInterface.MinDilation);
                return Mathf.Clamp(normalizedFloat, 0, 1);
            }, "EyesDilation"),

            new XYEyeParameter(v2 => v2.Left.Look, "LeftEyeX", "LeftEyeY"),
            new XYEyeParameter(v2 => v2.Right.Look, "RightEyeX", "RightEyeY"),

            new FloatEyeParameter(v2 => v2.Left.Openness, "LeftEyeLid", true),
            new FloatEyeParameter(v2 => v2.Right.Openness, "RightEyeLid", true),
            
            new BoolEyeParameter(v2 => v2.Left.Openness < 0.5f, "LeftEyeLid"),
            new BoolEyeParameter(v2 => v2.Right.Openness < 0.5f, "RightEyeLid"),
            
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
            new FloatEyeParameter(v2 => v2.Right.Squeeze, "RightEyeSqueeze")
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}