using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Types;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking;

public class MainStandalone : IMainService
{
    public IOSCService OscMain;
    public UnifiedConfig UnifiedConfig = new();

    public static readonly CancellationTokenSource MasterCancellationTokenSource = new();
    private readonly ILogger _logger;
    private readonly IAvatarInfo _avatarInfo;
    private readonly ILibManager _libManager;

    public Action<string, float> ParameterUpdate
    {
        get;
        set;
    } = (s, f) =>
    {
    };

    public MainStandalone(ILoggerFactory loggerFactory, IOSCService oscService, IAvatarInfo avatarInfo, ILibManager libManager)
    {
        _logger = loggerFactory.CreateLogger("MainStandalone");
        OscMain = oscService;
        _avatarInfo = avatarInfo;
        _libManager = libManager;
    }

    public void Teardown()
    {
        _logger.LogInformation("VRCFT Standalone Exiting!");
        _libManager.TeardownAllAndResetAsync();

        _logger.LogDebug("Saving configuration...");
        UnifiedConfig.Save();

        // Kill our threads
        _logger.LogDebug("Cancelling token sources...");
        MasterCancellationTokenSource.Cancel();
        
        _logger.LogDebug("Resetting our time end period...");
        Utils.TimeEndPeriod(1);
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
        
        // Try to load config and propogate data into Unified if they exist.
        _logger.LogDebug("Reading configuration...");
        UnifiedConfig.ReadConfiguration();

        ConfigParser.OnConfigLoaded += (relevantParams, configRaw) =>
        {
            _logger.LogDebug("Configuration loaded. Checking for native tracking parameters...");
            var hasLoadedNative = relevantParams.Any(p => p.GetParamNames().Any(t => t.paramName.StartsWith("/tracking/")));
            if (hasLoadedNative)
                _logger.LogWarning("Native tracking parameters detected.");
            var deprecatedParams = relevantParams.Count(p => p.Deprecated);

            _logger.LogInformation(relevantParams.Length + " parameters loaded.");
            if (deprecatedParams > 0)
                _logger.LogWarning(
                    deprecatedParams +
                    " Legacy parameters detected. " +
                    "Please consider updating the avatar to use the latest documented parameters.");

            _logger.LogDebug("Updating avatar info...");
            _avatarInfo.Id = configRaw.id;
            _avatarInfo.Name = configRaw.name;
            _avatarInfo.CurrentParameters = relevantParams.Length;
            _avatarInfo.CurrentParametersLegacy = deprecatedParams;
        };

        // Begin main OSC update loop
        _logger.LogDebug("Starting OSC update loop...");
        Utils.TimeBeginPeriod(1);
        ThreadPool.QueueUserWorkItem(ct =>
        {
            var token = (CancellationToken)ct;
            
            var buffer = new byte[4096];
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(10);

                UnifiedTracking.UpdateData();

                // Send all messages in OSCParams.SendQueue
                if (OSCParams.SendQueue.Count <= 0) continue;

                var relevantMessages = OSCParams.SendQueue.ToArray();
                var messageIndex = 0;
                while (messageIndex < relevantMessages.Length)
                {
                    var nextByteIndex = SROSCLib.create_osc_message(buffer, ref relevantMessages[messageIndex]);
                    if (nextByteIndex > 4096)
                    {
                        _logger.LogError("OSC message too large to send! Skipping this batch of messages.");
                        break;
                    }
                    
                    //if (relevantMessages[messageIndex].Type == OscValueType.Float)  // Little update function for debug parameter tab.
                    //    ParameterUpdate(relevantMessages[messageIndex].Address, relevantMessages[messageIndex].Value.FloatValues[0]);
                    
                    OscMain.Send(buffer, nextByteIndex);
                    messageIndex++;
                }

                OSCParams.SendQueue.Clear();
            }
        }, MasterCancellationTokenSource.Token);

        await Task.CompletedTask;
    }
}