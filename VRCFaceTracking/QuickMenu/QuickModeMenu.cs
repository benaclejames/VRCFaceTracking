using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

namespace VRCFaceTracking.QuickMenu
{
    public static class QuickModeMenu
    {
        private static bool _hasInitMenu;
        private static MenuStateController _menuStateController;
        private static Transform _menuTab;
        
        public static bool IsMenuShown => _menuStateController.field_Private_UIPage_0 == MainMenu.MainMenuPage;
        public static MainMenu MainMenu;
        private static AssetBundle _assetBundle;

        public static void LoadBundle() => _assetBundle = AssetBundle.LoadFromMemory(ExtractAb());
        
        public static void CheckIfShouldInit()
        {
            if (_hasInitMenu) return;
            
            CreateNotificationTab();
            _hasInitMenu = true;
        }

        private static void CreateNotificationTab()
        {
            var baseParent = GameObject.Find(
                    "UserInterface").transform
                .FindChild("Canvas_QuickMenu(Clone)/Container/Window/Page_Buttons_QM/HorizontalLayoutGroup");

            var dashboard = baseParent.FindChild("Page_Settings");
            _menuTab = GameObject.Instantiate(dashboard, baseParent);
            
            var sprite = LoadQmSprite(_assetBundle);
            var image = _menuTab.FindChild("Icon").GetComponent<Image>();
            image.sprite = sprite;
            image.overrideSprite = sprite;

            var menuTab = _menuTab.GetComponent<MenuTab>();
            menuTab.field_Private_MenuStateController_0 =
                dashboard.GetComponent<MenuTab>().field_Private_MenuStateController_0;
            menuTab.field_Public_String_0 = "QuickMenuFaceTracking";

            // Main Window
            var parentRoot = GameObject.Find("UserInterface").transform;
            var menuController = parentRoot.FindChild("Canvas_QuickMenu(Clone)");
            _menuStateController = menuController.GetComponent<MenuStateController>();
            var mainWindowParent = GameObject.Find("UserInterface").transform
                .FindChild("Canvas_QuickMenu(Clone)/Container/Window/QMParent");
            MainMenu = new MainMenu(mainWindowParent, _assetBundle, _menuStateController);
            
            // Add Streamer Mode Hide
            _menuTab.gameObject.SetActive(
                !VRCInputManager.Method_Public_Static_Boolean_InputSetting_0(
                    VRCInputManager.InputSetting.StreamerModeEnabled));

            GameObject
                .Find("UserInterface/MenuContent/Screens/Settings/ComfortSafetyPanel/StreamerModeToggle")
                .GetComponent<Toggle>().onValueChanged
                .AddListener((Action<bool>) (b => _menuTab.gameObject.SetActive(!b)));
        }

        private static Sprite LoadQmSprite(AssetBundle bundle)
        {
            var t = bundle.LoadAsset<Texture2D>("sranipal");
            var rect = new Rect(0.0f, 0.0f, t.width, t.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.zero;

            return Sprite.CreateSprite_Injected(t, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
        }
        
        private static byte[] ExtractAb()
        {
            var a = Assembly.GetExecutingAssembly();
            using (var resFilestream = a.GetManifestResourceStream("VRCFaceTracking.VRCFaceTracking"))
            {
                if (resFilestream == null) return null;
                var ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
    }
}
