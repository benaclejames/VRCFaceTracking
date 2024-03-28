using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.OSC;

[assembly:TypeForwardedTo(typeof(VRCFaceTracking.ExtTrackingModule))]
[assembly:TypeForwardedTo(typeof(VRCFaceTracking.ModuleMetadata))]
[assembly:TypeForwardedTo(typeof(ModuleState))]

namespace VRCFaceTracking;

public class MainStandalone : IMainService
{
    public static readonly CancellationTokenSource MasterCancellationTokenSource = new();
    
    private readonly OscQueryService _parameterOutputService;
    private readonly ILogger _logger;
    private readonly ILibManager _libManager;
    private readonly UnifiedTrackingMutator _mutator;

    public Action<string, float> ParameterUpdate { get; set; } = (_, _) => { };

    public MainStandalone(
        ILoggerFactory loggerFactory, 
        OscQueryService parameterOutputService,
        ILibManager libManager,
        UnifiedTrackingMutator mutator
        )
    {
        _logger = loggerFactory.CreateLogger("MainStandalone");
        _parameterOutputService = parameterOutputService;
        _libManager = libManager;
        _mutator = mutator;
    }

    public async Task Teardown()
    {
        _logger.LogInformation("VRCFT Standalone Exiting!");
        _libManager.TeardownAllAndResetAsync();

        await _mutator.SaveCalibration();

        // Kill our threads
        _logger.LogDebug("Cancelling token sources...");
        MasterCancellationTokenSource.Cancel();
        
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