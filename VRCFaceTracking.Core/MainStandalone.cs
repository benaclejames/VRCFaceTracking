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
    private readonly ILogger<MainStandalone> _logger;
    private readonly ILibManager _libManager;
    private readonly UnifiedTrackingMutator _mutator;

    public Action<string, float> ParameterUpdate { get; set; } = (_, _) => { };

    public MainStandalone(
        ILogger<MainStandalone> logger, 
        ILibManager libManager,
        UnifiedTrackingMutator mutator
        )
    {
        _logger = logger;
        _libManager = libManager;
        _mutator = mutator;
    }

    public async Task Teardown()
    {
        _logger.LogInformation("VRCFT Standalone Exiting!");
        await _mutator.Save();
        
        _libManager.TeardownAllAndResetAsync();
        
        _logger.LogDebug("Resetting our time end period...");
        var timeEndRes = Utils.TimeEndPeriod(1);
        if (timeEndRes != 0)
        {
            _logger.LogWarning($"TimeEndPeriod failed with HRESULT {timeEndRes}");
        }
        
        _logger.LogDebug("Teardown complete. Awaiting exit...");
    }

    public Task InitializeAsync()
    {
        // Ensure OSC is enabled
        if (OperatingSystem.IsWindows() && VRChat.ForceEnableOsc()) // If osc was previously not enabled
        {
            _logger.LogWarning("VRCFT detected OSC was disabled and automatically enabled it.");
            // If we were launched after VRChat
            if (VRChat.IsVrChatRunning())
                _logger.LogError(
                    "However, VRChat was running while this change was made.\n" +
                    "If parameters do not update, please restart VRChat or manually enable OSC yourself in your avatar's expressions menu.");
        }
        
        _mutator.Load();
        
        // Begin main OSC update loop
        _logger.LogDebug("Starting OSC update loop...");
        Utils.TimeBeginPeriod(1);
        return Task.CompletedTask;
    }
}