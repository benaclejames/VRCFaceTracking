namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        IParameter[] ResetParam(ConfigParser.Parameter[] newParams);
        bool Deprecated { get; }
    }
}