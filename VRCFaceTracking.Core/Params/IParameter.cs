namespace VRCFaceTracking.Params
{
    public interface IParameter
    {
        IParameter[] ResetParam(ConfigParser.Parameter[] newParams);
        (string paramName, IParameter paramLiteral)[] GetParamNames();
        bool Deprecated { get; }
    }
}