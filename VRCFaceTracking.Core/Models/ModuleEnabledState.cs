namespace VRCFaceTracking.Core.Models;

/// <summary>
/// The enabled state of a module, controlling which tracking slots it may claim.
/// Persisted as its numeric value.
/// </summary>
public enum ModuleEnabledState : byte
{
    /// <summary>
    /// Module is loaded and may claim both the eye and facial tracking slots.
    /// </summary>
    Enabled = 0,

    /// <summary>
    /// Module is not loaded at all.
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Module is loaded but may only claim the eye tracking slot.
    /// </summary>
    EyesOnly = 2,

    /// <summary>
    /// Module is loaded but may only claim the facial / expression tracking slot.
    /// </summary>
    FaceOnly = 3
}