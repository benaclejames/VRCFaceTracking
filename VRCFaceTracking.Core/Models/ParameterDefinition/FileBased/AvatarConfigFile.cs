using System.Text.Json.Serialization;
using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.Models.ParameterDefinition.FileBased;

public class AvatarConfigFile : IAvatarInfo
{
    public string id { get; set; }
    public string name { get; set; }
    public bool fullFaceTracking { get; set; }
    public AvatarConfigFileParameter[] parameters { get; set; }

    [JsonIgnore] public string Name => name;

    [JsonIgnore] public string Id => id;
    [JsonIgnore] public bool FullFaceTracking => fullFaceTracking;

    [JsonIgnore] public IParameterDefinition[] Parameters => parameters;
}