using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Desktop.Services;

namespace VRCFaceTracking.Desktop;

public class DesktopPlatformServices : IPlatformServices
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<IVrRuntimeService, OpenVRService>());
        services.Replace(ServiceDescriptor.Singleton<INotificationService, DesktopNotificationService>());
    }
}
