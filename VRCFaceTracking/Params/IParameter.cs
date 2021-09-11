namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        bool IsName(string name);
        void ResetParam();
        void ZeroParam();
    }
}