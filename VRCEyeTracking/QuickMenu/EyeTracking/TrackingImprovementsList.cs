using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;

namespace VRCEyeTracking.QuickMenu.EyeTracking
{
    public class TrackingImprovementsList
    {
        private readonly Text _improvementText;
        
        public TrackingImprovementsList(Transform root) => _improvementText = root.GetComponent<Text>();
        
        public void UpdateImprovements(TrackingImprovements improvements)
        {
            var improvementsText = "";
            for (int i = 0; i < improvements.count; i++)
                improvementsText += $"\n{improvements.items[i]}";
            _improvementText.text = improvementsText;
        }
    }
}