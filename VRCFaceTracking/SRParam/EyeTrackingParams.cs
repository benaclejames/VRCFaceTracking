using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

namespace VRCFaceTracking.SRParam
{
    public static class EyeTrackingParams
    {
        public static readonly List<ISRanipalParam> ParameterList = new List<ISRanipalParam>
        {
            new SRanipalXYEyeParameter(v2 =>
            {
                if (!v2.verbose_data.combined.eye_data.GetValidity(SingleEyeDataValidity
                    .SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)) return null;
                
                return Vector3.Scale(
                    v2.verbose_data.combined.eye_data.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
            }, "EyesX", "EyesY"),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide >
                                                  v2.expression_data.right.eye_wide
                ? v2.expression_data.left.eye_wide
                : v2.expression_data.right.eye_wide, "EyesWiden"),

            new SRanipalGeneralEyeParameter(v2 =>
            {
                var normalizedFloat = SRanipalTrack.CurrentDiameter / SRanipalTrack.MinDilation /
                                      (SRanipalTrack.MaxDilation - SRanipalTrack.MinDilation);
                return Mathf.Clamp(normalizedFloat, 0, 1);
            }, "EyesDilation"),

            new SRanipalXYEyeParameter(v2 =>
            {
                if (!v2.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                    return null;
                    
                return Vector3.Scale(
                    v2.verbose_data.left.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
            }, "LeftEyeX", "LeftEyeY"),

            new SRanipalXYEyeParameter(v2 =>
            {
                if (!v2.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                    return null;
                
                return Vector3.Scale(
                    v2.verbose_data.right.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
            }, "RightEyeX", "RightEyeY"),

            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.left.eye_openness, "LeftEyeLid", true),
            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.right.eye_openness, "RightEyeLid", true),
            
            new SRanipalGeneralEyeParameter(v2 =>
            {
                if (v2.verbose_data.left.eye_openness >= 1 && v2.expression_data.left.eye_wide > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.expression_data.left.eye_wide); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.verbose_data.left.eye_openness);
            }, "LeftEyeLidExpanded", true),
            new SRanipalGeneralEyeParameter(v2 =>
            {
                if (v2.verbose_data.right.eye_openness >= 1 && v2.expression_data.right.eye_wide > 0)
                    return NormalizeFloat(0, 1, 0.8f, 1, v2.expression_data.right.eye_wide); 
                return NormalizeFloat(0, 1, 0, 0.8f, v2.verbose_data.right.eye_openness);
            }, "RightEyeLidExpanded", true),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide, "LeftEyeWiden"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_wide, "RightEyeWiden"),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_squeeze, "LeftEyeSqueeze"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_squeeze, "RightEyeSqueeze")
        };

        // Brain Hurty
        private static float NormalizeFloat(float minInput, float maxInput, float minOutput, float maxOutput,
            float value) => (maxOutput - minOutput) / (maxInput - minInput) * (value - maxInput) + maxOutput;
    }
}