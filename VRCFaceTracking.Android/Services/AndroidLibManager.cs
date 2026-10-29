using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Android.Services;

// The desktop UnifiedLibManager hosts every module in its own VRCFaceTracking.ModuleProcess executable.
// Android apps can't spawn arbitrary executables, so until an Android module host exists this reports that
// no modules can be loaded rather than crashing on the missing sandbox binary.
public class AndroidLibManager(ILogger<AndroidLibManager> logger, IDispatcherService dispatcherService) : ILibManager
{
    public ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; } = new();

    public Task Initialize()
    {
        logger.LogWarning("Tracking modules are not supported on Android yet.");
        dispatcherService.Run(() =>
        {
            LoadedModulesMetadata.Clear();
            LoadedModulesMetadata.Add(new ModuleMetadataInternal
            {
                Active = false,
                Name = "Modules not supported on Android",
            });
        });
        return Task.CompletedTask;
    }

    public Task TeardownAllModules() => Task.CompletedTask;
}
