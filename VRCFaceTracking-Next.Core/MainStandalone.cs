using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Types;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking;

public class MainStandalone : IMainService
{
    public static OscMain OscMain;
    public static UnifiedConfig unifiedConfig = new();

    public static readonly CancellationTokenSource MasterCancellationTokenSource = new();

    public void Teardown()
    {
        unifiedConfig.Save();

        // Kill our threads
        MasterCancellationTokenSource.Cancel();
        
        Utils.TimeEndPeriod(1);
        Logger.Msg("VRCFT Standalone Exiting!");
        UnifiedLibManager.TeardownAllAndReset();
        Console.WriteLine("Shutting down");
    }

    public void Initialize()
    {
        Logger.Msg("VRCFT Initializing!");

        // Parse Arguments
        (var outPort, var ip, var inPort, var enableEye, var enableExpression) = ArgsHandler.HandleArgs();

        // Ensure OSC is enabled
        if (VRChat.ForceEnableOsc()) // If osc was previously not enabled
        {
            Logger.Warning("VRCFT detected OSC was disabled and automatically enabled it.");
            // If we were launched after VRChat
            if (VRChat.IsVRChatRunning())
                Logger.Error(
                    "However, VRChat was running while this change was made.\n" +
                    "If parameters do not update, please restart VRChat or manually enable OSC yourself in your avatar's expressions menu.");
        }

        // Load all available modules.
        UnifiedLibManager.ReloadModules();

        // Try to load config and propogate data into Unified if they exist.
        unifiedConfig.ReadConfiguration();

        // Initialize Tracking Runtimes
        UnifiedLibManager.SetTrackingEnabled(enableEye, enableExpression);
        UnifiedLibManager.Initialize();

        // Initialize Locals
        OscMain = new OscMain();
        var bindResults = OscMain.Bind(ip, outPort, inPort);
        if (!bindResults.receiverSuccess)
            Logger.Error(
                "Socket failed to bind to receiver port, please ensure it's not already in use by another program or specify a different one instead.");

        if (!bindResults.senderSuccess)
            Logger.Error(
                "Socket failed to bind to sender port, please ensure it's not already in use by another program or specify a different one instead.");

        ConfigParser.OnConfigLoaded += relevantParams =>
        {
            var deprecatedParams = relevantParams.Count(p => p.Deprecated);

            Logger.Msg(relevantParams.Length + " parameters loaded.");
            if (deprecatedParams > 0)
                Logger.Warning(
                    deprecatedParams +
                    " Legacy parameters detected. " +
                    "Please consider updating the avatar to use the latest documented parameters.");

        };

        // Begin main OSC update loop
        Utils.TimeBeginPeriod(1);
        while (!MasterCancellationTokenSource.IsCancellationRequested)
        {
            Thread.Sleep(1000);
            Logger.Msg("AAA");

            //if (_relevantParamsCount <= 0)
            //    continue;

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
    }
}