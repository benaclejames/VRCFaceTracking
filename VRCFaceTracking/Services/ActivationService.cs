using System.Reflection;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;

using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Services;

public class ActivationService(
    OscQueryService parameterOutputService,
    IMainService mainService,
    IModuleDataService moduleDataService,
    ModuleInstaller moduleInstaller,
    ILibManager libManager,
    ILogger<ActivationService> logger,
    IVrRuntimeService vrRuntimeService,
    INotificationService notificationService)
    : IActivationService
{
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();


        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);


        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        
    }

    private async Task InitializeAsync()
    {

        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        logger.LogInformation("VRCFT Version {version} initializing...", Assembly.GetExecutingAssembly().GetName().Version);
        
        logger.LogInformation("Initializing OSC...");
        await parameterOutputService.InitializeAsync();

        logger.LogInformation("Initializing main service...");
        await mainService.InitializeAsync();
        
        logger.LogInformation("Initializing VR runtime integration...");
        if (!vrRuntimeService.Initialize())
        {
            logger.LogWarning("No VR runtime integration available during ActivationService startup. Skipping.");
        }

        logger.LogInformation("Checking for updates for installed modules...");
        var localModules = moduleDataService.GetInstalledModules().Where(m => m.ModuleId != Guid.Empty);
        var remoteModules = await moduleDataService.GetRemoteModules();
        var outdatedModules = remoteModules.Where(rm => localModules.Any(lm =>
        {
            if (rm.ModuleId != lm.ModuleId || lm.IsLocal) 
                return false;
            
            try
            {
                var remoteVersion = new Version(rm.Version);
                var localVersion = new Version(lm.Version);

                return remoteVersion.CompareTo(localVersion) > 0;
            }
            catch
            {
                // Fall back to just string matching
                return string.CompareOrdinal(rm.Version, lm.Version) > 0;
            }
        }));
        foreach (var outdatedModule in outdatedModules)
        {
            logger.LogInformation($"Updating {outdatedModule.ModuleName} from {localModules.First(rm => rm.ModuleId == outdatedModule.ModuleId).Version} to {outdatedModule.Version}");
            if (await moduleInstaller.InstallRemoteModule(outdatedModule) != null)
            {
                notificationService.Show(
                    Strings.Resources.Notification_ModuleUpdated_Title,
                    string.Format(Strings.Resources.Notification_ModuleUpdated_Message, outdatedModule.ModuleName, outdatedModule.Version),
                    NotificationSeverity.Success);
            }
        }
        
        logger.LogInformation("Initializing modules...");
        Dispatcher.UIThread.Post(async () => await libManager.Initialize());
        
        await Task.CompletedTask;
    }
}
