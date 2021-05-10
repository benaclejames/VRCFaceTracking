using UnityEngine;
using UnityEngine.UI;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenu : SRanipalTrackingMenu
    {
        private readonly Image _lipImage;
        
        public LipTrackingMenu(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            _lipImage = pageRoot.Find("LipImage/Image").GetComponent<Image>();
            
            TrackingToggle.OnToggle += b => SRanipalTrack.LipEnabled = b;
            OnModuleReInitPress += () => SRanipalTrack.Initialize(false, true);
        }

        public void UpdateImage(Texture2D latestImage)
        {
            if (_lipImage == null) return;
            
            var rect = new Rect(0.0f, 0.0f, latestImage.width, latestImage.height);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.zero;

            _lipImage.sprite = Sprite.CreateSprite_Injected(latestImage, ref rect, ref pivot, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
        }
    }
}