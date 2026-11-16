using System.Collections.ObjectModel;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILibManager
{
    ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; }
    Task Initialize();
    Task TeardownAllModules();
}