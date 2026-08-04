namespace VRCFaceTracking.Core.Models;

public enum InstallState
{
    NotInstalled,
    Installed,
    Outdated,
    [Obsolete("No longer needed since we use sandboxing")]
    AwaitingRestart
}