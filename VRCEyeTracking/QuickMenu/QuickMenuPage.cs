using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRCEyeTracking.QuickMenu
{
    public class QuickMenuPage
    {
        public GameObject MenuObject;
        private List<QuickMenuButton> menuButtons = new List<QuickMenuButton>();
        
        public QuickMenuPage(QuickMenuTab menuTab, Transform menuObject)
        {
            MenuObject = new GameObject(menuTab.TabName + " Page");
            MenuObject.transform.parent = menuObject;

            MenuObject.AddComponent<Canvas>();
            MenuObject.AddComponent<GraphicRaycaster>();
            MenuObject.AddComponent<CanvasGroup>();
        }

        public QuickMenuButton CreateMenuButton(string buttonName, Vector2 buttonPosition, Action buttonPressAction,
            string buttonTooltip = null)
        {
            var newButton = new QuickMenuButton(this, buttonName, buttonPosition, buttonPressAction, buttonTooltip);
            menuButtons.Add(newButton);
            return newButton;
        }
    }
}