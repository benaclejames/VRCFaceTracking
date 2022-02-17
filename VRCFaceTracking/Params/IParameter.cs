using ParamLib;

namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        string[] GetName();
        
        // Rescan
        void ResetParam();
        
        // Reset Index to null
        void ZeroParam();
        BaseParam[] GetBase();
    }
}