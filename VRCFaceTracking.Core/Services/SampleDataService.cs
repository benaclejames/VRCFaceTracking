using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Services;

// This class holds sample data used by some generated pages to show how they can be used.
// TODO: The following classes have been created to display sample data. Delete these files once your app is using real data.
// 1. Contracts/Services/ISampleDataService.cs
// 2. Services/SampleDataService.cs
// 3. Models/SampleCompany.cs
// 4. Models/SampleOrder.cs
// 5. Models/SampleOrderDetail.cs
public class SampleDataService : ISampleDataService
{
    private List<RemoteTrackingModule> _allOrders;

    public SampleDataService()
    {
    }

    private static IEnumerable<RemoteTrackingModule> AllOrders()
    {
        // The following is order summary data
        return AllCompanies();
    }

    private static IEnumerable<RemoteTrackingModule> AllCompanies()
    {
        return new List<RemoteTrackingModule>()
        {
            new RemoteTrackingModule()
            {
                AuthorName = "Dazbme",
                ModuleName = "VRCFaceTracking-LiveLink",
                DownloadUrl = "https://github.com/Dazbme/VRCFaceTracking-LiveLink/releases/latest/",
                ModulePageUrl = "https://github.com/Dazbme/VRCFaceTracking-LiveLink",
                ModuleDescription = "This is a module for the VRCFaceTracking mod that enables you to use the FaceID sensors found on the IPhone X and newer to have face tracking with compatible avatars in VRChat desktop mode.",
                Version = "LiveLink-v2.2",
                Rating = 4,
                Downloads = 69420
            },
        };
    }

    public async Task<IEnumerable<RemoteTrackingModule>> GetListDetailsDataAsync()
    {
        if (_allOrders == null)
        {
            _allOrders = new List<RemoteTrackingModule>(AllOrders());
        }

        await Task.CompletedTask;
        return _allOrders;
    }
}
