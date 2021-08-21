using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.QuickMenu.LipTracking
{
    public class LipTrackingMenuPage : MenuPage
    {
        private readonly RawImage _lipImage;
        private readonly Texture2D _lipTexture;
        
        public LipTrackingMenuPage(Transform pageRoot, Transform lipTab) : base(pageRoot, lipTab.gameObject)
        {
            _lipImage = pageRoot.Find("LipImage/Image").GetComponent<RawImage>();
            _lipTexture = new Texture2D(SRanipal_Lip.ImageWidth, SRanipal_Lip.ImageHeight, TextureFormat.Alpha8, false);
            _lipImage.texture = _lipTexture;
            
            TrackingToggle.OnToggle += b => UnifiedLibManager.LipEnabled = b;
        }

        public void UpdateImage(IntPtr latestImage)
        {
            if (latestImage == IntPtr.Zero || _lipTexture == null || _lipImage == null) return;

            _lipTexture.LoadRawTextureData(latestImage, SRanipal_Lip.ImageWidth * SRanipal_Lip.ImageHeight);
            _lipTexture.Apply();
        }
    }
}