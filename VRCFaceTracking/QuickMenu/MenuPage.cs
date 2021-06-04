using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu
{
    public class MenuPage
    {
        internal readonly ToggleButton TrackingToggle;

        private static Action<GameObject> _onMenuTabPress = o => { };
        public readonly GameObject Root, TabObject;

        protected MenuPage(Transform pageRoot, GameObject tabObject)
        {
            Root = pageRoot.gameObject;
            TabObject = tabObject;
            
            TrackingToggle = new ToggleButton(pageRoot.Find("UtilButtons/ToggleActive"));

            _onMenuTabPress += o => Root.SetActive(o == Root);
            TabObject.GetComponent<Button>().onClick.AddListener((Action) (() => { _onMenuTabPress.Invoke(Root); }));
        }
    }
}