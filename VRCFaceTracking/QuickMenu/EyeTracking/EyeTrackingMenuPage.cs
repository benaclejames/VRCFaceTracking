using System;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;
using Vector2 = VRCFaceTracking.Params.Vector2;

namespace VRCFaceTracking.QuickMenu.EyeTracking
{
    public class EyeTrackingMenuPage : MenuPage
    {
        private readonly XYVisualizer _leftEyeVisualizer, _rightEyeVisualizer;
        private readonly Texture2D _eyeTexture;

        public EyeTrackingMenuPage(Transform pageRoot, Transform eyeTab) : base(pageRoot, eyeTab.gameObject)
        {
            var leftEye = pageRoot.Find("EyeIndicators/LeftEye/Mask/XYLines");
            var rightEye = pageRoot.Find("EyeIndicators/RightEye/Mask/XYLines");
            
            _leftEyeVisualizer = new XYVisualizer(leftEye.Find("X"), leftEye.Find("Y"));
            _rightEyeVisualizer = new XYVisualizer(rightEye.Find("X"), rightEye.Find("Y"));

            TrackingToggle.OnToggle += b => UnifiedLibManager.EyeEnabled = b;
            
            pageRoot.Find("UtilButtons/Recalibrate").GetComponent<Button>().onClick.AddListener((Action)(() => SRanipal_Eye_v2.LaunchEyeCalibration()));
            
            var eyeImage = pageRoot.Find("EyeImage/Image").GetComponent<RawImage>();
            _eyeTexture = new Texture2D(UnifiedTrackingData.LatestEyeData.ImageSize.x, UnifiedTrackingData.LatestEyeData.ImageSize.y, 
                TextureFormat.Alpha8, false);
            eyeImage.texture = _eyeTexture;
            
            if (UnifiedTrackingData.LatestEyeData.SupportsImage)
                eyeImage.transform.parent.gameObject.SetActive(true);
        }

        public void UpdateEyeTrack(EyeTrackingData eyeData)
        {
            UpdateLook(eyeData.Left.Look, eyeData.Right.Look);
            _eyeTexture.LoadRawTextureData(eyeData.ImageData);
            _eyeTexture.Apply();
        }

        private void UpdateLook(Vector2? leftEye, Vector2? rightEye)
        {
            // read memory from sr runtime
            if (leftEye.HasValue) _leftEyeVisualizer.ReceiveXY(leftEye.Value.x, leftEye.Value.y);
            if (rightEye.HasValue) _rightEyeVisualizer.ReceiveXY(rightEye.Value.x, rightEye.Value.y);
        }
    }
}