namespace VRCFaceTracking.Core.Contracts.Services;

public interface IParameterDefinition
{
    public string Address { get; }
    public string Name { get; }

    public Type Type { get; }
}