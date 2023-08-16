namespace VRCFaceTracking.Core.Params
{
    public interface IParameter
    {
        IParameter[] ResetParam((string paramName, string paramAddress, Type paramType)[] newParams);
        (string paramName, IParameter paramLiteral)[] GetParamNames();
        bool Deprecated { get; }
    }
}