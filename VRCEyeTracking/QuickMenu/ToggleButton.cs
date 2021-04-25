using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace VRCEyeTracking.QuickMenu
{
    public class ToggleButton
    {
        public static Sprite ToggleUp, ToggleDown;

        public Action<bool> OnToggle;

        private bool _currentState = true;
        private readonly Image _buttonImage;

        public ToggleButton(Transform buttonRoot)
        {
            OnToggle = OnToggleButton;
            _buttonImage = buttonRoot.gameObject.GetComponent<Image>();
            var uiButton = buttonRoot.gameObject.GetComponent<Button>();
            uiButton.onClick.AddListener((Action)(() => OnToggle.Invoke(!_currentState)));
        }

        private void OnToggleButton(bool newState)
        {
            _currentState = newState;
            _buttonImage.sprite = _currentState ? ToggleUp : ToggleDown;
        }
    }
}