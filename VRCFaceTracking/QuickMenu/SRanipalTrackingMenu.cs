using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu
{
    public class SRanipalTrackingMenu
    {
        internal readonly ToggleButton TrackingToggle;

        internal Action OnModuleReInitPress = () => { };
        private static Action<GameObject> _onMenuTabPress = o => { };
        public readonly GameObject Root, TabObject;

        protected SRanipalTrackingMenu(Transform pageRoot, GameObject tabObject)
        {
            Root = pageRoot.gameObject;
            TabObject = tabObject;
            
            TrackingToggle = new ToggleButton(pageRoot.Find("UtilButtons/ToggleActive"));
            pageRoot.Find("UtilButtons/ForceReInit").GetComponent<Button>().onClick.AddListener((Action) (() => {OnModuleReInitPress.Invoke();}));

            _onMenuTabPress += o => Root.SetActive(o == Root);
            TabObject.GetComponent<Button>().onClick.AddListener((Action) (() => { _onMenuTabPress.Invoke(Root); }));
        }
    }
}