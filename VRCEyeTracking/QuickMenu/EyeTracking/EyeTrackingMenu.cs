using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;

namespace VRCEyeTracking.QuickMenu.EyeTracking
{
    public class EyeTrackingMenu
    {
        private readonly XYVisualizer _leftEyeVisualizer, _rightEyeVisualizer;
        private readonly ToggleButton _trackingToggle;
        
        public readonly GameObject Root;
        
        public EyeTrackingMenu(Transform pageRoot)
        {
            Root = pageRoot.gameObject;
            var leftEye = pageRoot.Find("EyeIndicators/LeftEye/Mask/XYLines");
            var rightEye = pageRoot.Find("EyeIndicators/RightEye/Mask/XYLines");
            
            _leftEyeVisualizer = new XYVisualizer(leftEye.Find("X"), leftEye.Find("Y"));
            _rightEyeVisualizer = new XYVisualizer(rightEye.Find("X"), rightEye.Find("Y"));

            _trackingToggle = new ToggleButton(pageRoot.Find("UtilButtons/ToggleActive"));
            _trackingToggle.OnToggle += b => SRanipalTrack.EyeEnabled = b;
            
            pageRoot.Find("UtilButtons/Recalibrate").GetComponent<Button>().onClick.AddListener((Action)(() => SRanipal_Eye_v2.LaunchEyeCalibration()));
        }

        public void UpdateEyeTrack(EyeData_v2 eyeData)
        {
            UpdateXY(Vector3.Scale(
                eyeData.verbose_data.left.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), Vector3.Scale(
                eyeData.verbose_data.right.gaze_direction_normalized,
                new Vector3(-1, 1, 1)));
        }

        private void UpdateXY(Vector2 leftEye, Vector2 rightEye)
        {
            _leftEyeVisualizer.ReceiveXY(leftEye.x, leftEye.y);
            _rightEyeVisualizer.ReceiveXY(rightEye.x, rightEye.y);
        }
    }
}