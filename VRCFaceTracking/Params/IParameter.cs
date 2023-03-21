using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        int ResetParam(ConfigParser.Parameter[] newParams);

        //OSCParams.ParameterState[] GetSelfAndChildren();
    }
}