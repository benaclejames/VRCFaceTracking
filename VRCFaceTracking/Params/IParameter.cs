using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        void ResetParam(ConfigParser.Parameter[] newParams);
        
        OSCBaseParams.BaseParam[] GetBase();
    }
}