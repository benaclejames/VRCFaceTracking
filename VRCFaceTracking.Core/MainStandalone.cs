using Microsoft.Extensions.Logging;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Types;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking;

public class MainStandalone : IMainService
{
    public IOSCService OscMain;
    public UnifiedConfig UnifiedConfig = new();

    public static readonly CancellationTokenSource MasterCancellationTokenSource = new();
    private readonly ILogger _logger;
    
    public MainStandalone(ILoggerFactory loggerFactory, IOSCService oscService)
    {
        _logger = loggerFactory.CreateLogger("MainStandalone");
        OscMain = oscService;
    }

    public void Teardown()
    {
        UnifiedConfig.Save();

        // Kill our threads
        MasterCancellationTokenSource.Cancel();
        
        Utils.TimeEndPeriod(1);
        _logger.LogInformation("VRCFT Standalone Exiting!");
        UnifiedLibManager.TeardownAllAndReset();
        Console.WriteLine("Shutting down");
    }

    public void SetEnabled(bool enabledEyes, bool enabledExpression)
    {
        UnifiedLibManager.SetTrackingEnabled(enabledEyes, enabledExpression);
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("VRCFT Initializing!");

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

        // Load all available modules.
        UnifiedLibManager.ReloadModules();
        
        // Try to load config and propogate data into Unified if they exist.
        UnifiedConfig.ReadConfiguration();

        // Initialize Tracking Runtimes
        UnifiedLibManager.SetTrackingEnabled(true, true);
        UnifiedLibManager.Initialize();

        ConfigParser.OnConfigLoaded += relevantParams =>
        {
            var deprecatedParams = relevantParams.Count(p => p.Deprecated);

            _logger.LogInformation(relevantParams.Length + " parameters loaded.");
            if (deprecatedParams > 0)
                _logger.LogError(
                    deprecatedParams +
                    " Legacy parameters detected. " +
                    "Please consider updating the avatar to use the latest documented parameters.");

        };

        // Begin main OSC update loop
        Utils.TimeBeginPeriod(1);
        ThreadPool.QueueUserWorkItem(new WaitCallback(ct =>
        {
            var token = (CancellationToken)ct;
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
                    var buffer = new byte[4096];
                    var length = SROSCLib.create_osc_bundle(buffer, relevantMessages, relevantMessages.Length,
                        ref messageIndex);
                    if (length > 4096)
                        throw new Exception("Bundle size is too large! This should never happen.");

                    OscMain.Send(buffer, length);
                }

                OSCParams.SendQueue.Clear();
            }
        }), MasterCancellationTokenSource.Token);

        await Task.CompletedTask;
    }
}