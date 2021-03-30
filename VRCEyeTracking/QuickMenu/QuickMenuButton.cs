using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCEyeTracking.QuickMenu
{
    public class QuickMenuButton
    {
        private static GameObject ButtonTemplate => GameObject.Find("UserInterface/QuickMenu/QuickModeMenus/QuickModeInviteResponseMoreOptionsMenu/BlockButton");

        private GameObject _buttonObject;
        private Text _buttonText;
        private UiTooltip _tooltip;
        private RectTransform _rectTransform;
        

        private GameObject ButtonObject
        {
            get => _buttonObject;
            set
            {
                _rectTransform = value.GetComponent<RectTransform>();
                _buttonText = value.GetComponentInChildren<Text>();
                _buttonObject = value;
            }
        }

        public QuickMenuButton(QuickMenuPage parentPage, string buttonName, Vector2 buttonPosition, Action buttonPressAction,
            string buttonTooltip = null)
        {
            /*ButtonObject = Object.Instantiate(ButtonTemplate, parentPage.MenuObject.transform, true);
            _buttonText.text = buttonName;
            ButtonObject.name = buttonName;
            _rectTransform.anchoredPosition = ConvertMenuSpaceToLocalAnchor(buttonPosition);*/
        }

        private static Vector2 ConvertMenuSpaceToLocalAnchor(Vector2 menuPosition)
        {
            // Base Positions
            var baseVector = new Vector2(49.81381f, -48.88406f);

            // Apply offsets
            baseVector.x += menuPosition.x * 0.12f;

            return baseVector;
        }
    }
}