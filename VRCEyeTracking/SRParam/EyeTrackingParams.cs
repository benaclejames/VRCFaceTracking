using System.Collections.Generic;
using UnityEngine;

namespace VRCEyeTracking.SRParam
{
    public static class EyeTrackingParams
    {
        public static readonly List<ISRanipalParam> ParameterList = new List<ISRanipalParam>
        {
            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.combined.eye_data.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "EyesX", "EyesY"),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide >
                                                  v2.expression_data.right.eye_wide
                ? v2.expression_data.left.eye_wide
                : v2.expression_data.right.eye_wide, "EyesWiden"),

            new SRanipalGeneralEyeParameter(v2 =>
            {
                var normalizedFloat = SRanipalTrack.CurrentDiameter / SRanipalTrack.MinOpen /
                                      (SRanipalTrack.MaxOpen - SRanipalTrack.MinOpen);
                return Mathf.Clamp(normalizedFloat, 0, 1);
            }, "EyesDilation"),

            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.left.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "LeftEyeX", "LeftEyeY"),

            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.right.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "RightEyeX", "RightEyeY"),

            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.left.eye_openness, "LeftEyeLid", true),
            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.right.eye_openness, "RightEyeLid", true),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide, "LeftEyeWiden"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_wide, "RightEyeWiden"),

            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_squeeze, "LeftEyeSqueeze"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_squeeze, "RightEyeSqueeze")
        };
    }
}