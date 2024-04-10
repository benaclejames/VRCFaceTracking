using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.Models.ParameterDefinition;

/// <summary>
/// NullAvatarDef is used when we don't really have a legitimate avatar definition and need to construct one at runtime.
/// This allows us to ensure the UI has something to display to the user about the avatar.
/// </summary>
public class NullAvatarDef : IAvatarInfo
{
    private readonly string _name, _id;
    
    public NullAvatarDef(string name, string id)
    {
        _name = name;
        _id = id;
    }

    public string Name => _name;

    public string Id => _id;

    public IParameterDefinition[] Parameters { get; }
}