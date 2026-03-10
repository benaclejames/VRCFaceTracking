using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;

[assembly: TypeForwardedTo(typeof(VRCFaceTracking.ExtTrackingModule))]
[assembly: TypeForwardedTo(typeof(VRCFaceTracking.ModuleMetadata))]
[assembly: TypeForwardedTo(typeof(ModuleState))]

namespace VRCFaceTracking.Core;

public class MainStandalone(
    ILogger<MainStandalone> logger,
    ILibManager libManager,
    UnifiedTrackingMutator mutator)
    : IMainService
{
    public Action<string, float> ParameterUpdate { get; set; } = (_, _) => { };

    public async Task Teardown()
    {
        logger.LogInformation("VRCFT Standalone Exiting!");
        await mutator.Save();

        libManager.TeardownAllAndResetAsync();

        if (OperatingSystem.IsWindows())
        {
            logger.LogDebug("Resetting our time end period...");
            var timeEndRes = Utils.TimeEndPeriod(1);
            if (timeEndRes != 0)
            {
                logger.LogWarning($"TimeEndPeriod failed with HRESULT {timeEndRes}");
            }
        }

        logger.LogDebug("Teardown complete. Awaiting exit...");
    }

    public Task InitializeAsync()
    {
        VRChat.EnsureVRCOSCDirectory();

        if (VRChat.ForceEnableOsc()) // If osc was previously not enabled
        {
            logger.LogWarning("VRCFT detected OSC was disabled and automatically enabled it.");
            // If we were launched after VRChat
            if (VRChat.IsVrChatRunning())
                logger.LogError(
                    "However, VRChat was running while this change was made.\n" +
                    "If parameters do not update, please restart VRChat or manually enable OSC yourself in your avatar's expressions menu.");
        }

        mutator.Load();

        // Begin main OSC update loop
        logger.LogDebug("Starting OSC update loop...");
        
        Utils.TimeBeginPeriod(1);
        
        return Task.CompletedTask;
    }
}
