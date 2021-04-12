using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace VRCEyeTracking.QuickMenu
{
    public static class QuickModeMenu
    {
        public static bool HasInitMenu;
        public static QuickMenuTab EyeTab, MouthTab;

        private static GameObject OriginalTabsObject =>
            GameObject.Find("UserInterface/QuickMenu/QuickModeMenus/QuickModeNotificationsMenu/NotificationTabs");


        public static void InitializeMenu()
        {
             CreateNotificationTab("VRCSRanipal", "Text", Color.green,  // Organization 100
                    "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");
             HasInitMenu = true;
        }

        private static (Transform tab, Transform menu) CreateNotificationTab(string name, string text, Color color, string imageDataBase64 = null)
        {
            List<GameObject> existingTabs = Resources.FindObjectsOfTypeAll<MonoBehaviourPublicObCoGaCoObCoObCoUnique>()[0].field_Public_ArrayOf_GameObject_0.ToList();

            global::QuickMenu quickMenu = Resources.FindObjectsOfTypeAll<global::QuickMenu>()[0];

            // Tab

            MonoBehaviourPublicObCoGaCoObCoObCoUnique quickModeTabs = quickMenu.transform.Find("QuickModeTabs").GetComponent<MonoBehaviourPublicObCoGaCoObCoObCoUnique>();
            Transform newTab = GameObject.Instantiate(quickModeTabs.transform.Find("NotificationsTab"), quickModeTabs.transform);
            newTab.name = name;
            GameObject.DestroyImmediate(newTab.GetComponent<MonoBehaviourPublicGaTeSiSiUnique>());
            SetTabIndex(newTab, (MonoBehaviourPublicObCoGaCoObCoObCoUnique.EnumNPublicSealedvaHoNoPl4vUnique)existingTabs.Count);
            newTab.Find("Badge").GetComponent<RawImage>().color = color;
            newTab.Find("Badge/NotificationsText").GetComponent<Text>().text = text;

            existingTabs.Add(newTab.gameObject);

            Resources.FindObjectsOfTypeAll<MonoBehaviourPublicObCoGaCoObCoObCoUnique>()[0].field_Public_ArrayOf_GameObject_0 = existingTabs.ToArray();

            if (imageDataBase64 != null)
                newTab.Find("Icon").GetComponent<Image>().sprite = CreateSpriteFromBase64(imageDataBase64);
            else
                newTab.Find("Icon").gameObject.SetActive(false);

            // Menu

            Transform quickModeMenus = quickMenu.transform.Find("QuickModeMenus");
            RectTransform newMenu = new GameObject(name + "Menu", new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() }).GetComponent<RectTransform>();
            newMenu.SetParent(quickModeMenus, false);
            newMenu.anchorMin = new Vector2(0, 1);
            newMenu.anchorMax = new Vector2(0, 1);
            newMenu.sizeDelta = new Vector2(1680f, 1200f);
            newMenu.pivot = new Vector2(0.5f, 0.5f);
            newMenu.anchoredPosition = new Vector2(0, 200f);
            newMenu.gameObject.SetActive(false);
            
            HandleMenuTabCreation(newMenu);

            // Tab interaction
            var tabButton = newTab.GetComponent<Button>();
            tabButton.onClick.RemoveAllListeners();
            tabButton.onClick.AddListener((Action)(() =>
            {
                global::QuickMenu.prop_QuickMenu_0.field_Private_GameObject_6.SetActive(false);
                global::QuickMenu.prop_QuickMenu_0.field_Private_GameObject_6 = newMenu.gameObject;
                newMenu.gameObject.SetActive(true);
            }));
            
            newTab.transform.FindChild("Badge").gameObject.SetActive(false);

            // Allow invite menu to instantiate
            quickModeMenus.Find("QuickModeNotificationsMenu").gameObject.SetActive(true);
            quickModeMenus.Find("QuickModeNotificationsMenu").gameObject.SetActive(false);

            return (newTab, newMenu);
        }

        private static void SetTabIndex(Transform tab, MonoBehaviourPublicObCoGaCoObCoObCoUnique.EnumNPublicSealedvaHoNoPl4vUnique value)
        {
            MonoBehaviour tabDescriptor = tab.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().GetMethod("ShowTabContent") != null);

            tabDescriptor.GetIl2CppType().GetFields().First(f => f.FieldType.IsEnum).SetValue(tabDescriptor, new Il2CppSystem.Int32 { m_value = (int)value }.BoxIl2CppObject());
        }


        private static Sprite CreateSpriteFromBase64(string data)
        {
            Texture2D t = new Texture2D(2, 2);
            ImageConversion.LoadImage(t, Convert.FromBase64String(data));
            Rect rect = new Rect(0.0f, 0.0f, t.width, t.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Vector4 border = Vector4.zero;

            Sprite s = Sprite.CreateSprite_Injected(t, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight, ref border, false);

            return s;
        }

        private static void HandleMenuTabCreation(Transform newMenu)
        {
            var newTabs =
                Object.Instantiate(OriginalTabsObject, OriginalTabsObject.transform.parent, true);

            newTabs.name = "SRanipalTabs";

            newTabs.transform.localScale = OriginalTabsObject.transform.localScale;
            newTabs.transform.localPosition = OriginalTabsObject.transform.localPosition;
            newTabs.transform.localRotation = OriginalTabsObject.transform.localRotation;

            //Strip away notification menu scripts and instantiate top buttons
            for (var i=0; i < newTabs.transform.GetChildCount(); i++)
            {
                var tab = newTabs.transform.GetChild(i).gameObject;

                switch (tab.gameObject.name)
                {
                    case "InvitesTab":
                        EyeTab = new QuickMenuTab(tab.gameObject, "Eye Tracking", "View the Eye Tracking Menu");
                        EyeTab.TabEnabled = SRanipalTrack.EyeEnabled; // Catch up with SRanipal
                        break;
                    case "FriendRequestsTab":
                        MouthTab = new QuickMenuTab(tab.gameObject, "Mouth Tracking", "View the Mouth Tracking Menu");
                        MouthTab.TabEnabled = SRanipalTrack.FaceEnabled; // Catch up with SRanipal
                        break;
                    default:
                        Object.Destroy(tab.gameObject);
                        continue;
                }
            }

            newTabs.transform.parent = newMenu;
            
            HandlePageCreation(EyeTab, newMenu);
        }

        private static void HandlePageCreation(QuickMenuTab menuTab, Transform menuObject)
        {
            var newPage = new QuickMenuPage(menuTab, menuObject);
            newPage.CreateMenuButton("DoAThing", new Vector2(0, 0), () => MelonLogger.Msg("BUTTON PRESS"));

        }
    }
}
