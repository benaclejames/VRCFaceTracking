using System.Collections.ObjectModel;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILibManager
{
    public ObservableCollection<ModuleMetadata> ModuleMetadatas { get; set; }
    public void Initialize();
    void TeardownAllAndResetAsync();
}