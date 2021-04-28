using System.Collections.Generic;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRParam
{
    public interface ISRanipalParam
    {
        void RefreshParam(EyeData_v2? eyeData, Dictionary<LipShape_v2, float> lipData = null);
        void ResetParam();
        void ZeroParam();
    }
}