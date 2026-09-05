using Microsoft.Windows.ApplicationModel.Resources;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Helpers;

/// <summary>
/// Provides localized display text for <see cref="ModuleEnabledState"/> values.
/// </summary>
public static class ModuleStateStrings
{
    private static readonly ResourceLoader Loader = new ResourceLoader();

    public static string Localize(ModuleEnabledState state) => state switch
    {
        ModuleEnabledState.Enabled => Loader.GetString("ModuleStateText_Enabled"),
        ModuleEnabledState.Disabled => Loader.GetString("ModuleStateText_Disabled"),
        ModuleEnabledState.EyesOnly => Loader.GetString("ModuleStateText_EyesOnly"),
        ModuleEnabledState.FaceOnly => Loader.GetString("ModuleStateText_FaceOnly"),
        _ => Loader.GetString("ModuleStateText_Enabled")
    };

    /// <summary>
    /// Builds the module list badge text. Shows "(old → new)" when the module's state has been
    /// changed during this session and a restart is pending.
    /// </summary>
    public static string BuildBadge(ModuleEnabledState? applied, ModuleEnabledState current)
    {
        if (applied.HasValue && applied.Value != current)
        {
            return $"({Localize(applied.Value)} → {Localize(current)})";
        }

        return Localize(current);
    }
}