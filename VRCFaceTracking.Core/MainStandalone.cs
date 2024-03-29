using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;

[assembly:TypeForwardedTo(typeof(VRCFaceTracking.ExtTrackingModule))]
[assembly:TypeForwardedTo(typeof(VRCFaceTracking.ModuleMetadata))]
[assembly:TypeForwardedTo(typeof(ModuleState))]

namespace VRCFaceTracking.Core;

public class MainStandalone : IMainService
{
    private readonly ILogger _logger;
    private readonly ILibManager _libManager;
    private readonly UnifiedTrackingMutator _mutator;

    public Action<string, float> ParameterUpdate { get; set; } = (_, _) => { };

    public MainStandalone(
        ILoggerFactory loggerFactory, 
        ILibManager libManager,
        UnifiedTrackingMutator mutator
        )
    {
        _logger = loggerFactory.CreateLogger("MainStandalone");
        _libManager = libManager;
        _mutator = mutator;
    }

    public async Task Teardown()
    {
        _logger.LogInformation("VRCFT Standalone Exiting!");
        _libManager.TeardownAllAndResetAsync();

        await _mutator.SaveCalibration();
        
        _logger.LogDebug("Resetting our time end period...");
        Utils.TimeEndPeriod(1);
        
        _logger.LogDebug("Teardown successful. Awaiting exit...");
    }

    public Task InitializeAsync()
    {
        // Ensure OSC is enabled
        if (VRChat.ForceEnableOsc()) // If osc was previously not enabled
        {
            _logger.LogWarning("VRCFT detected OSC was disabled and automatically enabled it.");
            // If we were launched after VRChat
            if (VRChat.IsVRChatRunning())
                _logger.LogError(
                    "However, VRChat was running while this change was made.\n" +
                    "If parameters do not update, please restart VRChat or manually enable OSC yourself in your avatar's expressions menu.");
        }
        
        _mutator.LoadCalibration();
        
        // Begin main OSC update loop
        _logger.LogDebug("Starting OSC update loop...");
        Utils.TimeBeginPeriod(1);
        return Task.CompletedTask;
    }
}