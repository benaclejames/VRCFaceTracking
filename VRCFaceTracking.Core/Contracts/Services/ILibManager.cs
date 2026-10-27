using System.Collections.ObjectModel;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILibManager
{
    ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; }
    Task Initialize();
    Task TeardownAllModules();

    /// <summary>
    /// The module enabled states that were actually applied at the last initialize.
    /// </summary>
    IReadOnlyDictionary<string, ModuleEnabledState> AppliedModuleStates { get; }

    /// <summary>
    /// True when a saved module state differs from the state that is currently applied.
    /// </summary>
    bool IsReloadPending { get; }

    /// <summary>
    /// Raised whenever <see cref="IsReloadPending"/> changes.
    /// </summary>
    event EventHandler? ReloadPendingChanged;

    /// <summary>
    /// Records a newly saved module state and updates <see cref="IsReloadPending"/>.
    /// </summary>
    void NotifyModuleStateChanged(string moduleKey, ModuleEnabledState state);

    /// <summary>
    /// Re-initializes the modules so saved state changes take effect without restarting the app.
    /// </summary>
    Task ReloadModulesAsync();
}