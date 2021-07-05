using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Pimax;
using VRCFaceTracking.SRanipal;

namespace VRCFaceTracking
{
    public readonly struct Eye
    {
        private readonly Vector2? _look;
        private readonly float _openness;
        public readonly float Widen, Squeeze;

        public Eye(SingleEyeData eyeData, SingleEyeExpression? expression = null)
        {
            _look = null;
            _openness = 0;
            Widen = 0;
            Squeeze = 0;

            if (eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                _look = Vector3.Scale(
                    eyeData.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));

            _openness = eyeData.eye_openness;
            if (expression == null) return;
            
            Widen = expression.Value.eye_wide;
            Squeeze = expression.Value.eye_squeeze;
        }

        public Eye(EyeExpressionState eyeState)
        {
            _look = new Vector2(eyeState.PupilCenterX, eyeState.PupilCenterY);
            _openness = eyeState.Openness;
            Widen = 0;
            Squeeze = 0;
        }

        public static implicit operator Vector2?(Eye eye) => eye._look;
        public static implicit operator float(Eye eye) => eye._openness;
    }
    
    public struct EyeTrackingData
    {
        public Eye Left, Right, Combined;
        
        // SRanipal Exclusive
        public float EyesDilation;


        public static implicit operator EyeTrackingData(EyeData_v2 eyeData)
        {
            float dilation;
            
            if (eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.right.pupil_diameter_mm;
                SRanipalTrackingInterface.UpdateMinMaxDilation(eyeData.verbose_data.right.pupil_diameter_mm);
            }
            else if (eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.left.pupil_diameter_mm;
                SRanipalTrackingInterface.UpdateMinMaxDilation(eyeData.verbose_data.left.pupil_diameter_mm);
            }
            else dilation = eyeData.verbose_data.combined.eye_data.pupil_diameter_mm;
            
            return new EyeTrackingData
            {
                Left = new Eye(eyeData.verbose_data.left, eyeData.expression_data.left),
                Right = new Eye(eyeData.verbose_data.right, eyeData.expression_data.right),
                Combined = new Eye(eyeData.verbose_data.combined.eye_data),
                
                EyesDilation = dilation
            };
        }

        public static implicit operator EyeTrackingData(Ai1EyeData eyeData)
        {
            return new EyeTrackingData
            {
                Left = new Eye(eyeData.Left),
                Right = new Eye(eyeData.Right),
                Combined = new Eye(eyeData.Recommended)
            };
        }
    }

    public static class UnifiedTrackingData
    {
        public static EyeTrackingData LatestEyeData;
        public static LipData_v2 LatestLipData;
        public static Dictionary<LipShape_v2, float> LatestLipShapes;
    }
}