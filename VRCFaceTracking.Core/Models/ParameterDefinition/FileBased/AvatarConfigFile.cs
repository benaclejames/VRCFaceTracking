using System.ComponentModel;
using System.Text.Json.Serialization;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models.Osc.FileBased;

public class AvatarConfigFile : IAvatarInfo
{
    public string id { get; set; }
    public string name { get; set; }
    public AvatarConfigFileParameter[] parameters { get; set; }

    [JsonIgnore] public string Name => name;

    [JsonIgnore] public string Id => id;

    [JsonIgnore] public AvatarConfigFileParameter[] Parameters => parameters;
}