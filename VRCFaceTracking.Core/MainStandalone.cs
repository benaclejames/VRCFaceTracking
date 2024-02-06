using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking;

public class MainStandalone : IMainService
{
    public static readonly CancellationTokenSource MasterCancellationTokenSource = new();
    
    private readonly ParameterOutputService _parameterOutputService;
    private readonly ILogger _logger;
    private readonly ILibManager _libManager;
    private readonly UnifiedTrackingMutator _mutator;

    public Action<string, float> ParameterUpdate { get; set; } = (_, _) => { };

    public MainStandalone(
        ILoggerFactory loggerFactory, 
        ParameterOutputService parameterOutputService,
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

    public async Task InitializeAsync()
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

        _parameterOutputService.OnAvatarLoaded += (configRaw, discoveredParameters) =>
        {
            _logger.LogDebug("Configuration loaded. Checking for native tracking parameters...");
            var hasLoadedNative = discoveredParameters.Any(p => p.GetParamNames().Any(t => t.paramName.StartsWith("/tracking/")));
            if (hasLoadedNative)
                _logger.LogWarning("Native tracking parameters detected.");
            var deprecatedParams = discoveredParameters.Count(p => p.Deprecated);

            _logger.LogInformation(discoveredParameters.Count + " parameters loaded.");
            if (deprecatedParams > 0)
                _logger.LogWarning(
                    deprecatedParams +
                    " Legacy parameters detected. " +
                    "Please consider updating the avatar to use the latest documented parameters.");
        };
        
        _mutator.LoadCalibration();

        // Begin main OSC update loop
        _logger.LogDebug("Starting OSC update loop...");
        Utils.TimeBeginPeriod(1);
        ThreadPool.QueueUserWorkItem(ct =>
        {
            var token = (CancellationToken)ct;
            
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(10);

                UnifiedTracking.UpdateData();

                // Send all messages in OSCParams.SendQueue
                if (ParamSupervisor.SendQueue.Count <= 0) continue;

                while (ParamSupervisor.SendQueue.TryDequeue(out var message))
                {
                    
                    
                    //if (relevantMessages[messageIndex].Type == OscValueType.Float)  // Little update function for debug parameter tab.
                    //    ParameterUpdate(relevantMessages[messageIndex].Address, relevantMessages[messageIndex].Value.FloatValues[0]);
                    
                    _parameterOutputService.Send(message);
                }

                ParamSupervisor.SendQueue.Clear();
            }
        }, MasterCancellationTokenSource.Token);

        await Task.CompletedTask;
    }
}