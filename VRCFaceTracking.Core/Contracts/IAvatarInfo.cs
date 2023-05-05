namespace VRCFaceTracking.Core.Contracts.Services;

public interface IAvatarInfo
{
    public string Name { get; set; }
    public string Id { get; set; }
    public int CurrentParameters { get; set; }
    public int CurrentParametersLegacy { get; set; }
    public bool HasAnyLegacy { get; }
}