using System.Collections.Generic;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using VRC.UI.Elements;
using VRCFaceTracking.QuickMenu.EyeTracking;
using VRCFaceTracking.QuickMenu.LipTracking;

namespace VRCFaceTracking.QuickMenu
{
    public class MainMenu
    {
        private readonly EyeTrackingMenuPage _eyeTrackingMenuPage;
        private readonly LipTrackingMenuPage _lipTrackingMenuPage;
        public readonly UIPage MainMenuPage;
        private readonly Transform _errorPage;

        public MainMenu(Transform parentMenuTransform, AssetBundle bundle, MenuStateController controller)
        {
            // Instantiate and setup positioning
            var menuPrefab = bundle.LoadAsset<GameObject>("VRCSRanipal");
            var menuObject = Object.Instantiate(menuPrefab, parentMenuTransform, false);
            menuObject.name = "Menu_QuickMenuFaceTracking";
            menuObject.transform.localScale = new Vector3(1, 1, 1);
            menuObject.transform.localRotation = new Quaternion(0, 0, 0, 1);
            menuObject.transform.localPosition = new Vector3(0, 512, 0);
            
            // Setup MenuStateController and notify of new tab
            var menuStateController = Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>().FirstOrDefault()?.GetComponent<MenuStateController>();
            
            MainMenuPage = menuObject.AddComponent<UIPage>();
            MainMenuPage.field_Public_String_0 = "QuickMenuFaceTracking";
            MainMenuPage.field_Private_Boolean_1 = true;
            MainMenuPage.field_Private_MenuStateController_0 = menuStateController;
            MainMenuPage.field_Private_List_1_UIPage_0 = new Il2CppSystem.Collections.Generic.List<UIPage>();
            MainMenuPage.field_Private_List_1_UIPage_0.Add(MainMenuPage);
            
            if (menuStateController != null)
            {
                menuStateController.field_Private_Dictionary_2_String_UIPage_0.Add("QuickMenuFaceTracking",
                    MainMenuPage);

                var list = menuStateController.field_Public_ArrayOf_UIPage_0.ToList();
                list.Add(MainMenuPage);
                menuStateController.field_Public_ArrayOf_UIPage_0 = new Il2CppReferenceArray<UIPage>(list.ToArray());
            }

            menuObject.SetActive(false);

            _eyeTrackingMenuPage = new EyeTrackingMenuPage(menuObject.transform.Find("Pages/Eye Tracking"), menuObject.transform.Find("Tabs/Buttons/Eye Tracking"));
            _lipTrackingMenuPage = new LipTrackingMenuPage(menuObject.transform.Find("Pages/Lip Tracking"), menuObject.transform.Find("Tabs/Buttons/Lip Tracking"));
            _errorPage = menuObject.transform.Find("Pages/No Tracking Available");
            
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
            _errorPage.gameObject.SetActive(false);
            _eyeTrackingMenuPage.TabObject.SetActive(eye);
            _lipTrackingMenuPage.TabObject.SetActive(lip);
            
            if (eye)
                _eyeTrackingMenuPage.Root.SetActive(true);
            else if (lip)
                _lipTrackingMenuPage.Root.SetActive(true);
            else
                _errorPage.gameObject.SetActive(true);
        }

        public void UpdateParams()
        {
            if (_eyeTrackingMenuPage.Root.active) _eyeTrackingMenuPage.UpdateEyeTrack(UnifiedTrackingData.LatestEyeData);
            if (_lipTrackingMenuPage.Root.active) _lipTrackingMenuPage.UpdateImage(UnifiedTrackingData.LatestLipData.image);
        }
    }
}