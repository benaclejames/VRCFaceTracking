using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.QuickMenu.EyeTracking;
using Object = UnityEngine.Object;

namespace VRCEyeTracking.QuickMenu
{
    public class FaceTrackingMenu
    {
        private readonly EyeTrackingMenu _eyeTrackingMenu;
        private readonly GameObject _eyeTab, _lipTab;

        public FaceTrackingMenu(Transform parentMenuTransform, AssetBundle bundle)
        {
            var menuPrefab = bundle.LoadAsset<GameObject>("VRCSRanipal");
            var menuObject = Object.Instantiate(menuPrefab);
            menuObject.transform.parent = parentMenuTransform;
            menuObject.transform.localPosition = Vector3.zero;
            menuObject.transform.localScale = Vector3.oneVector;
            menuObject.transform.localRotation = new Quaternion(0, 0, 0, 1);

            _eyeTrackingMenu = new EyeTrackingMenu(menuObject.transform.Find("Pages/EyeTracking"));
            
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
            
            _eyeTab = menuObject.transform.Find("Tabs/Buttons/Eye Tracking").gameObject;
            _eyeTab.GetComponent<Button>().onClick.AddListener((Action)(() =>
            {
                _eyeTrackingMenu.Root.SetActive(true);
            }));
            
            _lipTab = menuObject.transform.Find("Tabs/Buttons/Lip Tracking").gameObject;
            
            UpdateEnabledTabs(SRanipalTrack.EyeEnabled, SRanipalTrack.FaceEnabled);
        }

        public void UpdateEnabledTabs(bool eye = false, bool lip = false)
        {
            _eyeTab.SetActive(eye);
            _lipTab.SetActive(lip);
            
            if (eye)
                _eyeTrackingMenu.Root.SetActive(true);
            //else if (lip)
        }

        public void UpdateParams(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null)
        {
            if (_eyeTrackingMenu.Root.active && eyeData.HasValue) _eyeTrackingMenu.UpdateEyeTrack(eyeData.Value);
        }
    }
}