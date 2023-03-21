using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Types;

[assembly: AssemblyTitle("VRCFaceTracking")]
[assembly: AssemblyDescription("Application to enable Face Tracking from within VRChat using OSC")]
[assembly: AssemblyCompany("benaclejames")]
[assembly: AssemblyProduct("VRCFaceTracking")]
[assembly: AssemblyCopyright("Copyright © benaclejames 2022")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("3.0.1")]
[assembly: AssemblyFileVersion("3.0.1")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        public static OscMain OscMain;
        public static UnifiedConfig unifiedConfig = new UnifiedConfig();

        public static readonly CancellationTokenSource MasterCancellationTokenSource = new CancellationTokenSource();

        public static void Teardown()
        {
            unifiedConfig.Save();

            // Kill our threads
            MasterCancellationTokenSource.Cancel();
            
            Utils.TimeEndPeriod(1);
            Logger.Msg("VRCFT Standalone Exiting!");
            UnifiedLibManager.TeardownAllAndReset();
            Console.WriteLine("Shutting down");
            MainWindow.TrayIcon.Visible = false;
            Application.Current?.Shutdown();
        }
        
        public static void Initialize()
        {
            var test2 = BitConverter.GetBytes(69.42);
            Logger.Msg("VRCFT Initializing!");
            
            // Parse Arguments
            (int outPort, string ip, int inPort, bool enableEye, bool enableExpression) = ArgsHandler.HandleArgs();

            // Ensure OSC is enabled
            if (VRChat.ForceEnableOsc())  // If osc was previously not enabled
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
                Logger.Error("Socket failed to bind to receiver port, please ensure it's not already in use by another program or specify a different one instead.");
            
            if (!bindResults.senderSuccess)
                Logger.Error("Socket failed to bind to sender port, please ensure it's not already in use by another program or specify a different one instead.");

            ConfigParser.OnConfigLoaded += (count) =>
            {
                    Logger.Warning(count + " legacy parameters loaded. These are undocumented and outdated parameters.");
            };

            // Begin main OSC update loop
            Utils.TimeBeginPeriod(1);
            while (!MasterCancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(10);

                //if (_relevantParamsCount <= 0)
                //    continue;

                UnifiedTracking.UpdateData();
                
                // Send all messages in OSCParams.SendQueue
                if (OSCParams.SendQueue.Count <= 0) continue;
                
                var relevantMessages = OSCParams.SendQueue.ToArray();
                int messageIndex = 0;
                while (messageIndex < relevantMessages.Length)
                {
                    byte[] buffer = new byte[4096];
                    var length = RustLib.create_osc_bundle(buffer, relevantMessages, relevantMessages.Length,
                        ref messageIndex);
                    if (length > 4096)
                        throw new Exception("Bundle size is too large! This should never happen.");

                    OscMain.Send(buffer, length);
                }
                OSCParams.SendQueue.Clear();
            }
        }
    }
}