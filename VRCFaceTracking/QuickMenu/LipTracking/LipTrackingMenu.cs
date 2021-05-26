using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenu : SRanipalTrackingMenu
    {
        private readonly RawImage _lipImage;
        
        public LipTrackingMenu(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            _lipImage = pageRoot.Find("LipImage/Image").GetComponent<RawImage>();
            
            TrackingToggle.OnToggle += b => UnifiedLibManager.LipEnabled = b;
            OnModuleReInitPress += () => new Thread(() => UnifiedLibManager.Initialize(false)).Start();
        }

        public void UpdateImage(Texture2D latestImage)
        {
            if (_lipImage == null) return;
            
            _lipImage.texture = latestImage;
        }
    }
}