using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Pimax;

namespace VRCFaceTracking
{
    public struct Eye
    {
        public Vector2? Look;
        public float Openness;
        public float Widen, Squeeze;

        public void Update(SingleEyeData eyeData, SingleEyeExpression? expression = null)
        {
            Look = null;
            Openness = 0;
            Widen = 0;
            Squeeze = 0;

            if (eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                Look = Vector3.Scale(
                    eyeData.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));

            Openness = eyeData.eye_openness;
            if (expression == null) return;
            
            Widen = expression.Value.eye_wide;
            Squeeze = expression.Value.eye_squeeze;
        }

        public void Update(EyeExpressionState eyeState)
        {
            Look = new Vector2(eyeState.PupilCenterX, eyeState.PupilCenterY);
            Openness = eyeState.Openness;
            Widen = 0;
            Squeeze = 0;
        }
    }
    
    public struct EyeTrackingData
    {
        public Eye Left, Right, Combined;
        
        // SRanipal Exclusive
        public float EyesDilation;
        private float _maxDilation, _minDilation;


        public void UpdateData(EyeData_v2 eyeData)
        {
            float dilation;
            
            if (eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.right.pupil_diameter_mm;
                UpdateMinMaxDilation(eyeData.verbose_data.right.pupil_diameter_mm);
            }
            else if (eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.left.pupil_diameter_mm;
                UpdateMinMaxDilation(eyeData.verbose_data.left.pupil_diameter_mm);
            }
            else dilation = eyeData.verbose_data.combined.eye_data.pupil_diameter_mm;

            Left.Update(eyeData.verbose_data.left, eyeData.expression_data.left);
            Right.Update(eyeData.verbose_data.right, eyeData.expression_data.right);
            Combined.Update(eyeData.verbose_data.combined.eye_data);

            EyesDilation = dilation;
        }

        public void UpdateData(Ai1EyeData eyeData)
        {
            Left.Update(eyeData.Left);
            Right.Update(eyeData.Right);
            Combined.Update(eyeData.Recommended);
        }

        private void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > _maxDilation)
                _maxDilation = readDilation;
            if (readDilation < _minDilation)
                _minDilation = readDilation;
        }
    }

    public struct UnifiedTrackingData
    {
        public static EyeTrackingData LatestEyeData;
        public static LipData_v2 LatestLipData;
        public static Dictionary<LipShape_v2, float> LatestLipShapes;
    }
}