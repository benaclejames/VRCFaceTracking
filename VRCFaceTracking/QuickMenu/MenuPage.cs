#if DLL
using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu
{
    public class MenuPage
    {
        internal readonly ToggleButton TrackingToggle;

        private static Action<GameObject> _onMenuTabPress = o => { };
        protected static Action OnReInitModulePress = () => MelonCoroutines.Start(UnifiedLibManager.CheckRuntimeSanity());
        public readonly GameObject Root, TabObject;

        protected MenuPage(Transform pageRoot, GameObject tabObject)
        {
            Root = pageRoot.gameObject;
            TabObject = tabObject;
            
            TrackingToggle = new ToggleButton(pageRoot.Find("UtilButtons/ToggleActive"));
            var reinit = pageRoot.Find("Recalibrate");
            reinit.gameObject.SetActive(true);
            reinit.GetComponent<Button>().onClick.AddListener((Action) (() => { OnReInitModulePress.Invoke(); }));

            _onMenuTabPress += o => Root.SetActive(o == Root);
            TabObject.GetComponent<Button>().onClick.AddListener((Action) (() => { _onMenuTabPress.Invoke(Root); }));
        }
    }
}
#endif