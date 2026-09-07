using Newtonsoft.Json;

namespace VRCFaceTracking.Core.Models;

public class InstallableTrackingModule : TrackingModuleMetadata
{
    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }
}