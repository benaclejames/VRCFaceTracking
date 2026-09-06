using System.Collections.ObjectModel;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILibManager
{
    public ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; }
    public void Initialize();
    void TeardownAllAndResetAsync();

    /// <summary>
    /// The module enabled states that were actually applied at the last initialize.
    /// Used to detect state changes that still require a restart to take effect.
    /// </summary>
    IReadOnlyDictionary<string, ModuleEnabledState> AppliedModuleStates { get; }
}