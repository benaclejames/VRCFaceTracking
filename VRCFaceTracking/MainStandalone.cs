using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.OSC;

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
        
        private static List<OscMessage> ConstructMessages(IEnumerable<OSCParams.BaseParam> parameters) => 
            parameters.Where(p => p.NeedsSend).Select(param =>
            {
                param.NeedsSend = false;
                return new OscMessage(param.OutputInfo.address, param.OscType, param.ParamValue);
            }).ToList();

        private static IEnumerable<OSCParams.BaseParam> _relevantParams;
        private static int _relevantParamsCount = 416;

        private static string _ip = "127.0.0.1";
        private static int _inPort = 9001, _outPort = 9000;

        public static readonly CancellationTokenSource MasterCancellationTokenSource = new CancellationTokenSource();

        public static void Teardown()
        {
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
            Logger.Msg("VRCFT Initializing!");
            
            // Parse Arguments
            (_outPort, _ip, _inPort) = ArgsHandler.HandleArgs();
            
            // Load dependencies
            DependencyManager.Load();

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
            
            // Initialize Tracking Runtimes
            UnifiedLibManager.Initialize();

            // Initialize Locals
            OscMain = new OscMain();
            var bindResults = OscMain.Bind(_ip, _outPort, _inPort);
            if (!bindResults.receiverSuccess)
                Logger.Error("Socket failed to bind to receiver port, please ensure it's not already in use by another program or specify a different one instead.");
            
            if (!bindResults.senderSuccess)
                Logger.Error("Socket failed to bind to sender port, please ensure it's not already in use by another program or specify a different one instead.");

            _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);

            ConfigParser.OnConfigLoaded += () =>
            {
                _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase())
                    .Where(param => param.Relevant);
                UnifiedTrackingData.LatestEyeData.ResetThresholds();
                _relevantParamsCount = _relevantParams.Count();
                Logger.Msg("Config file parsed successfully! " + _relevantParamsCount + " parameters loaded");
            };

            // Begin main OSC update loop
            Utils.TimeBeginPeriod(1);
            while (!MasterCancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(10);
                
                if (_relevantParamsCount <= 0)
                    continue;

                UnifiedTrackingData.OnUnifiedDataUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipData);

                var messages = ConstructMessages(_relevantParams);
                while (messages.Count > 0)
                {
                    var msgCount = 16;
                    var msgList = new List<OscMessage>();
                    while (messages.Count > 0 && msgCount+messages[0].Data.Length+4 < 4096)
                    {
                        msgList.Add(messages[0]);
                        msgCount += messages[0].Data.Length+4;
                        messages.RemoveAt(0);
                    }
                    var bundle = new OscBundle(msgList);
                    OscMain.Send(bundle.Data);
                }
                
            }
        }
    }
}