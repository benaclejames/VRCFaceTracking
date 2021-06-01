using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenuPage : MenuPage
    {
        private readonly RawImage _lipImage;
        
        public LipTrackingMenuPage(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            _lipImage = pageRoot.Find("LipImage/Image").GetComponent<RawImage>();
            
            TrackingToggle.OnToggle += b => UnifiedLibManager.LipEnabled = b;
        }

        public void UpdateImage(Texture2D latestImage)
        {
            if (_lipImage == null) return;
            
            _lipImage.texture = latestImage;
        }
    }
}