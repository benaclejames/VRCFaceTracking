using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenuPage : MenuPage
    {
        public LipTrackingMenuPage(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            TrackingToggle.OnToggle += b => UnifiedLibManager.LipEnabled = b;
        }
    }
}