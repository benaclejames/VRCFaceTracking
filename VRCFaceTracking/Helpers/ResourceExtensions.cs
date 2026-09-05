using Microsoft.Windows.ApplicationModel.Resources;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);

    public static string Localize(ModuleEnabledState state) => state switch
    {
        ModuleEnabledState.Enabled => "ModuleStateText_Enabled".GetLocalized(),
        ModuleEnabledState.Disabled => "ModuleStateText_Disabled".GetLocalized(),
        ModuleEnabledState.EyesOnly => "ModuleStateText_EyesOnly".GetLocalized(),
        ModuleEnabledState.FaceOnly => "ModuleStateText_FaceOnly".GetLocalized(),
        _ => "ModuleStateText_Enabled".GetLocalized()
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

        return $"({Localize(current)})";
    }
}
