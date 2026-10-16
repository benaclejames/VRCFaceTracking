using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VRCFaceTracking.Android.Services;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Android;

public class AndroidPlatformServices : IPlatformServices
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<INotificationService, AndroidNotificationService>());

        // Android can't spawn the out-of-process module sandbox the desktop uses
        services.Replace(ServiceDescriptor.Singleton<ILibManager, AndroidLibManager>());

        services.AddHostedService<MulticastLockService>();

        // No PC VR runtime on Android, so the shared NullVrRuntimeService default stays registered
    }
}
