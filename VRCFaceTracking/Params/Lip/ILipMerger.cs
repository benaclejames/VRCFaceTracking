using System.Collections.Generic;
using ViveSR.anipal.Lip;


namespace VRCFaceTracking.Params.Lip
{
    interface ILipMerger
    {
        float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap);
    }
}
