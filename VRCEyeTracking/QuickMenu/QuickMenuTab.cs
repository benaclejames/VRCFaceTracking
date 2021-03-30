using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace VRCEyeTracking.QuickMenu
{
    public class QuickMenuTab
    {
        private GameObject _tabObject;
        private Text _buttonText;
        private GameObject _alertSprite;
        private Button _tabButton;
        private string _tabName;
        private string _buttonDisplayText;
        private bool _alertEnabled = true;


        private GameObject TabObject
        {
            get => _tabObject;
            set
            {
                SanitizeTab(value);
                _buttonText = value.GetComponentInChildren<Text>();
                _alertSprite = value.transform.Find("Text/NewBadge").gameObject;
                _tabButton = value.GetComponent<Button>();
                _tabButton.onClick.RemoveAllListeners();
                _tabObject = value;
            }
        }

        public string TabName
        {
            get => _tabName;
            set
            {
                if (_tabObject != null)
                    _tabObject.name = value;
                _tabName = value;
            }
        }

        private string ButtonDisplayText
        {
            get => _buttonDisplayText;
            set
            {
                if (_tabObject != null)
                {
                    if (_buttonText != null)
                        _buttonText.text = value;
                }
                _buttonDisplayText = value;
            }
        }

        private string UIToolTip
        {
            get => TabObject.GetComponent<UiTooltip>().field_Public_String_0;
            set => TabObject.GetComponent<UiTooltip>().field_Public_String_0 = value;
        }

        private bool AlertEnabled
        {
            get => _alertEnabled;
            set
            {
                if (_alertSprite == null) return;
                
                _alertSprite.active = value;
                _alertEnabled = value;
            }
        }

        public bool TabEnabled
        {
            get => _tabObject.active;
            set => _tabObject.active = value;
        }
        
        public QuickMenuTab(GameObject tabObject, string tabName, string tabDescription, string tabDisplayName = null)
        {
            TabObject = tabObject;
            TabName = tabName;

            SetupAppearance(tabDescription, tabDisplayName);
            
            // Set visibility to false to avoid false visibility for uninitialized modules
            TabEnabled = false;
        }

        private static void SanitizeTab(GameObject gameObject)
        {
            Object.Destroy(gameObject.GetComponent<ButtonReaction>());
            Object.Destroy(gameObject.GetComponent<MonoBehaviourPublicObTeGaBuSiStSiStStUnique>());
        }

        private void SetupAppearance(string tabDescription, string tabDisplayName = null)
        {
            UIToolTip = tabDescription;
            ButtonDisplayText = tabDisplayName ?? TabName;
            _tabObject.GetComponentInChildren<Image>().color =
                new Color(0.03137255f, 0.3764706f, 0.3921569f, 1);
            AlertEnabled = false;
        }

        public bool Bind(Action buttonPressAction)
        {
            if (_tabButton == null) return false;
            
            _tabButton.onClick.AddListener(buttonPressAction);
            return true;
        }
    }
}