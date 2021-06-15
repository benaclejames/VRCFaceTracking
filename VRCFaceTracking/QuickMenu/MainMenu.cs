using UnityEngine;
using VRCFaceTracking.QuickMenu.EyeTracking;
using VRCFaceTracking.QuickMenu.LipTracking;

namespace VRCFaceTracking.QuickMenu
{
    public class MainMenu
    {
        private readonly EyeTrackingMenuPage _eyeTrackingMenuPage;
        private readonly LipTrackingMenuPage _lipTrackingMenuPage;

        public MainMenu(Transform parentMenuTransform, AssetBundle bundle)
        {
            var menuPrefab = bundle.LoadAsset<GameObject>("VRCSRanipal");
            var menuObject = Object.Instantiate(menuPrefab, parentMenuTransform, true);
            menuObject.transform.localPosition = Vector3.zero;
            menuObject.transform.localScale = Vector3.oneVector;
            menuObject.transform.localRotation = new Quaternion(0, 0, 0, 1);

            _eyeTrackingMenuPage = new EyeTrackingMenuPage(menuObject.transform.Find("Pages/Eye Tracking"), menuObject.transform.Find("Tabs/Buttons/Eye Tracking"));
            _lipTrackingMenuPage = new LipTrackingMenuPage(menuObject.transform.Find("Pages/Lip Tracking"), menuObject.transform.Find("Tabs/Buttons/Lip Tracking"));
            
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
                switch (sprite.name)
                {
                    case "UI_ButtonToggleBottom_Bifrost":
                        ToggleButton.ToggleDown = sprite;
                        break;
                    case "UI_ButtonToggleTop_Bifrost":
                        ToggleButton.ToggleUp = sprite;
                        break;
                }
            
            UpdateEnabledTabs(UnifiedLibManager.EyeEnabled, UnifiedLibManager.LipEnabled);
        }

        public void UpdateEnabledTabs(bool eye = false, bool lip = false)
        {
            _eyeTrackingMenuPage.TabObject.SetActive(eye);
            _lipTrackingMenuPage.TabObject.SetActive(lip);
            
            if (eye)
                _eyeTrackingMenuPage.Root.SetActive(true);
            else if (lip)
                _lipTrackingMenuPage.Root.SetActive(true);
        }

        public void UpdateParams()
        {
            if (_eyeTrackingMenuPage.Root.active) _eyeTrackingMenuPage.UpdateEyeTrack(UnifiedTrackingData.LatestEyeData);
        }
    }
}