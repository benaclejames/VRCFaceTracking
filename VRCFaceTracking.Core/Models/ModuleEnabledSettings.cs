namespace VRCFaceTracking.Core.Models;

/// <summary>
/// Persisted per-module toggles controlling which parts of a module VRCFT may load.
/// </summary>
public class ModuleEnabledSettings
{
    /// <summary>
    /// Whether the module as a whole is allowed to load.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether the module is allowed to claim the eye tracking slot.
    /// </summary>
    public bool EnableEye { get; set; } = true;

    /// <summary>
    /// Whether the module is allowed to claim the facial / expression tracking slot.
    /// </summary>
    public bool EnableExpression { get; set; } = true;
}