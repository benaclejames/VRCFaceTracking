using System.Collections.ObjectModel;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface ILibManager
{
    public ObservableCollection<ModuleMetadata> ModuleMetadatas { get; set; }
    public Action<string[]> OnLoad { get; set; }
    public void Initialize();
    void TeardownAllAndReset();
}