namespace VRCFaceTracking.Core.Contracts.Services;

public interface IAvatarInfo
{
    public string Name { get; }
    public string Id { get; }
    public IParameterDefinition[] Parameters { get; }
}