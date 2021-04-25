using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.QuickMenu.EyeTracking;

namespace VRCEyeTracking.QuickMenu
{
    public class FaceTrackingMenu
    {
        private EyeTrackingMenu _eyeTrackingMenu;
        
        public FaceTrackingMenu(Transform parentMenuTransform)
        {
            var bundle = AssetBundle.LoadFromMemory(ExtractAb());
            var menuPrefab = bundle.LoadAsset<GameObject>("VRCSRanipal");
            var menuObject = Object.Instantiate(menuPrefab);
            menuObject.transform.parent = parentMenuTransform;
            menuObject.transform.localPosition = Vector3.zero;
            menuObject.transform.localScale = Vector3.oneVector;
            menuObject.transform.localRotation = new Quaternion(0, 0, 0, 1);

            _eyeTrackingMenu = new EyeTrackingMenu(menuObject.transform.Find("Pages/EyeTracking"));
        }
        
        private static byte[] ExtractAb()
        {
            var a = Assembly.GetExecutingAssembly();
            using (var resFilestream = a.GetManifestResourceStream("VRCEyeTracking.VRCFaceTracking"))
            {
                if (resFilestream == null) return null;
                var ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public void UpdateParams(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (_eyeTrackingMenu.Root.active && eyeData.HasValue) _eyeTrackingMenu.UpdateEyeTrack(eyeData.Value);
        }
    }
}