using Newtonsoft.Json;

namespace VRCFaceTracking.Core.Models;

public enum InstallState
{
    NotInstalled,
    Installed,
    Outdated,
    AwaitingRestart
}

public class InstallableTrackingModule : TrackingModuleMetadata
{
    public InstallState InstallationState
    {
        get; set;
    }
    
    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }
}