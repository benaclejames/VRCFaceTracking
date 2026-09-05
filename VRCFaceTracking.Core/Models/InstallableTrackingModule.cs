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

    /// <summary>
    /// Whether this module is enabled. Disabled modules are not loaded until VRCFT is restarted.
    /// The value is persisted separately and does not get written to module.json.
    /// </summary>
    [JsonIgnore]
    public bool IsEnabled
    {
        get; set;
    } = true;

    /// <summary>
    /// Whether this module is allowed to claim the eye tracking slot.
    /// </summary>
    [JsonIgnore]
    public bool EnableEye
    {
        get; set;
    } = true;

    /// <summary>
    /// Whether this module is allowed to claim the facial / expression tracking slot.
    /// </summary>
    [JsonIgnore]
    public bool EnableExpression
    {
        get; set;
    } = true;

    /// <summary>
    /// A stable identifier used to persist the enabled/disabled state of a module.
    /// Registry modules are keyed by their module id, legacy modules by their assembly path.
    /// </summary>
    [JsonIgnore]
    public string ModuleKey => ModuleId != Guid.Empty ? ModuleId.ToString() : AssemblyLoadPath;
}