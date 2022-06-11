using System;
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
            _lipTexture = new Texture2D(SRanipal_Lip_v2.ImageWidth, SRanipal_Lip_v2.ImageHeight, TextureFormat.Alpha8, false);
            _lipImage.texture = _lipTexture;
            
            TrackingToggle.OnToggle += b => UnifiedLibManager.LipStatus = b ? ModuleState.Active : ModuleState.Idle;
        }

        public void UpdateImage(byte[] latestImage)
        {
            if (latestImage == null || _lipTexture == null || _lipImage == null) return;

            _lipTexture.LoadRawTextureData(latestImage);
            _lipTexture.Apply();
        }
    }
}