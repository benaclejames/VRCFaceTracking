using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
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

            var notifTab = baseParent.FindChild("Page_Notifications");

            _menuTab = GameObject.Instantiate(notifTab, baseParent, true);

            _menuTab.GetComponent<MenuTab>().field_Public_String_0 = "QuickMenuFaceTracking";
            var iconImage = _menuTab.FindChild("Icon").GetComponent<Image>();
            
            GameObject.Destroy(_menuTab.GetComponent<MonoBehaviourPublicGaTeVoStVoAwOnVoVoVoUnique>()); // Deobfuscated name = NotificationTab
            GameObject.Destroy(_menuTab.GetComponent<StyleElement>());
            iconImage.sprite = LoadQmSprite(_assetBundle);
            iconImage.m_Color = new Color(0.4157f, 0.8902f, 0.9765f, 1);

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
