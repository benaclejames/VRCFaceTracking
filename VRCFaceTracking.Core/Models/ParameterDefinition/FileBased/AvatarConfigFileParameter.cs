using System.Text.Json.Serialization;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models.Osc.FileBased;

public class AvatarConfigFileParameter : IParameterDefinition
{
    public string name { get; set; }
    public AvatarConfigFileIODef input { get; set; }
    public AvatarConfigFileIODef output { get; set; }
    
    [JsonIgnore] public string Address => input.address;

    [JsonIgnore] public string Name => name;

    [JsonIgnore] public Type Type => input.Type;
}