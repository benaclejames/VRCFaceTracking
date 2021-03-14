using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

namespace VRCEyeTracking.SRParam
{
    public interface ISRanipalParam
    {
        void RefreshParam(EyeData_v2? eyeData, LipData_v2? lipData);
        void ResetParam();
    }
}