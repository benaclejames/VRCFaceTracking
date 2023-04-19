using Newtonsoft.Json;

namespace VRCFaceTracking.Core.Models;

public enum InstallState
{
    NotInstalled,
    Installed,
    Outdated
}

public class LocalTrackingModule : RemoteTrackingModule
{
    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }
}