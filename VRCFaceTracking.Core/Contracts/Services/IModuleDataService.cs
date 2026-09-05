using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface IModuleDataService
{
    Task<IEnumerable<InstallableTrackingModule>> GetRemoteModules();
    Task<int?> GetMyRatingAsync(TrackingModuleMetadata moduleMetadata);
    Task SetMyRatingAsync(TrackingModuleMetadata moduleMetadata, int rating);
    IEnumerable<InstallableTrackingModule> GetInstalledModules();
    Task IncrementDownloadsAsync(TrackingModuleMetadata moduleMetadata);
    IEnumerable<InstallableTrackingModule> GetLegacyModules();
}