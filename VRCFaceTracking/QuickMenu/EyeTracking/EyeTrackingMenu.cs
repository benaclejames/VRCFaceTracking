using System;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;

namespace VRCFaceTracking.QuickMenu.EyeTracking
{
    public class EyeTrackingMenu : SRanipalTrackingMenu
    {
        private readonly XYVisualizer _leftEyeVisualizer, _rightEyeVisualizer;

        public EyeTrackingMenu(Transform pageRoot, Transform eyeTab) : base(pageRoot, eyeTab.gameObject)
        {
            var leftEye = pageRoot.Find("EyeIndicators/LeftEye/Mask/XYLines");
            var rightEye = pageRoot.Find("EyeIndicators/RightEye/Mask/XYLines");
            
            _leftEyeVisualizer = new XYVisualizer(leftEye.Find("X"), leftEye.Find("Y"));
            _rightEyeVisualizer = new XYVisualizer(rightEye.Find("X"), rightEye.Find("Y"));

            TrackingToggle.OnToggle += b => SRanipalTrack.EyeEnabled = b;
            
            pageRoot.Find("UtilButtons/Recalibrate").GetComponent<Button>().onClick.AddListener((Action)(() => SRanipal_Eye_v2.LaunchEyeCalibration()));
        }

        public void UpdateEyeTrack(EyeData_v2 eyeData)
        {
            Vector3? leftGazeDir = null, rightGazeDir = null;
            if (eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                rightGazeDir = Vector3.Scale(
                    eyeData.verbose_data.right.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
            
            if (eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                leftGazeDir = Vector3.Scale(
                    eyeData.verbose_data.left.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
                
            UpdateXY(leftGazeDir, rightGazeDir);
        }

        private void UpdateXY(Vector2? leftEye, Vector2? rightEye)
        {
            if (leftEye.HasValue) _leftEyeVisualizer.ReceiveXY(leftEye.Value.x, leftEye.Value.y);
            if (rightEye.HasValue) _rightEyeVisualizer.ReceiveXY(rightEye.Value.x, rightEye.Value.y);
        }
    }
}