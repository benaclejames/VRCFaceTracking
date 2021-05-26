using UnityEngine;
using UnityEngine.UI;
using VRCFaceTracking.SRanipal;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenu : SRanipalTrackingMenu
    {
        private readonly RawImage _lipImage;
        
        public LipTrackingMenu(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            _lipImage = pageRoot.Find("LipImage/Image").GetComponent<RawImage>();
            
            TrackingToggle.OnToggle += b => SRanipalTrack.LipEnabled = b;
            OnModuleReInitPress += () => SRanipalTrack.Initialize(false, true);
        }

        public void UpdateImage(Texture2D latestImage)
        {
            if (_lipImage == null) return;
            
            _lipImage.texture = latestImage;
        }
    }
}