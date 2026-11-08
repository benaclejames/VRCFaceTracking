using Newtonsoft.Json;

namespace VRCFaceTracking.Core.Models;

public class InstallableTrackingModule : TrackingModuleMetadata
{
    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }

    /// <summary>
    /// A stable identifier used to persist the enabled/disabled state of a module.
    /// Registry modules are keyed by their module id, legacy modules by their assembly path.
    /// </summary>
    [JsonIgnore]
    public string ModuleKey => ModuleId != Guid.Empty ? ModuleId.ToString() : AssemblyLoadPath;
}