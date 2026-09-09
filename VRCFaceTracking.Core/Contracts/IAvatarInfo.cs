namespace VRCFaceTracking.Core.Contracts;

public interface IAvatarInfo
{
    string Name { get; }
    string Id { get; }
    bool FullFaceTracking { get; }
    IParameterDefinition[] Parameters { get; }
}