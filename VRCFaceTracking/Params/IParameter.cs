using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        void ResetParam(ConfigParser.Parameter[] newParams);
        
        OSCParams.BaseParam[] GetBase();
    }
}