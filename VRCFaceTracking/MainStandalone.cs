using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Text.Json;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.Params;
using VRCFaceTracking.OSC;
using System.IO;
using System.Text;
using VRCFaceTracking.Types;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

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

        private static string unifiedConfigPath = Utils.PersistentDataDirectory + "\\Config.json";

        private static List<OscMessage> ConstructMessages(IEnumerable<OSCParams.BaseParam> parameters) =>
            parameters.Where(p => p.NeedsSend).Select(param =>
            {
                param.NeedsSend = false;
                return new OscMessage(param.OutputInfo.address, param.OscType, param.ParamValue);
            }).ToList();

        private static IEnumerable<OSCParams.BaseParam> _relevantParams;
        private static IEnumerable<OSCParams.BaseParam> _relevantParams_v2;
        private static int _relevantParamsCount = 416;

        private static string _ip = "127.0.0.1";
        private static int _inPort = 9001, _outPort = 9000;

        public static readonly CancellationTokenSource MasterCancellationTokenSource = new CancellationTokenSource();

        public static void Teardown()
        {
            File.Delete(unifiedConfigPath);

            using (Stream stream = File.OpenWrite(unifiedConfigPath))
            {
                unifiedConfig.Data = UnifiedTracking.Data;
                unifiedConfig.RequestedModulePaths = new List<string>();
                UnifiedLibManager._requestedModules.ForEach(a => unifiedConfig.RequestedModulePaths.Add(a.Location));

                JsonSerializer.Serialize(stream, unifiedConfig, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
                stream.Dispose();
            }

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

            UnifiedLibManager.ReloadModules();

            // Try to load config and try to initialize runtimes if they exist.
            try
            {
                string jsonString = File.ReadAllText(unifiedConfigPath);
                unifiedConfig = JsonSerializer.Deserialize<UnifiedConfig>(jsonString, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });

                if (unifiedConfig != null)
                {
                    if (unifiedConfig.RequestedModulePaths.Count > 0)
                    {
                        Logger.Msg("Saved module load order initialized.");
                        UnifiedLibManager.LoadRequestedModulePaths(unifiedConfig.RequestedModulePaths.ToArray());
                        UnifiedLibManager.RequestInitialize();
                    }
                    else
                    {
                        Logger.Msg("Load order not found; initializing default order scheme.");
                        
                        // Initialize Tracking Runtimes
                        UnifiedLibManager.Initialize();
                    }

                    if (unifiedConfig.Data != null)
                    {
                        UnifiedTracking.Data = unifiedConfig.Data;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Warning("Configuration file not found, will be created on proper exit of application.");
            }

            // Initialize Locals
            OscMain = new OscMain();
            var bindResults = OscMain.Bind(_ip, _outPort, _inPort);
            if (!bindResults.receiverSuccess)
                Logger.Error("Socket failed to bind to receiver port, please ensure it's not already in use by another program or specify a different one instead.");
            
            if (!bindResults.senderSuccess)
                Logger.Error("Socket failed to bind to sender port, please ensure it's not already in use by another program or specify a different one instead.");

            _relevantParams = UnifiedTracking.AllParameters_v1.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
            _relevantParams_v2 = UnifiedTracking.AllParameters_v2.SelectMany(p => p.GetBase()).Where(param => param.Relevant);

            ConfigParser.OnConfigLoaded += () =>
            {
                _relevantParams = UnifiedTracking.AllParameters_v1.SelectMany(p => p.GetBase())
                    .Where(param => param.Relevant);
                _relevantParams_v2 = UnifiedTracking.AllParameters_v2.SelectMany(p => p.GetBase())
                    .Where(param => param.Relevant);

                // Reset calibration on parameter data.
                UnifiedTracking.Data.ResetCalibration();

                _relevantParamsCount = _relevantParams.Count() + _relevantParams_v2.Count();
                Logger.Msg("Config file parsed successfully! " + _relevantParamsCount + " parameters loaded.");
                
                if (_relevantParams.Count() > 0)
                    Logger.Warning(_relevantParams.Count() + " legacy parameters loaded. These are undocumented and outdated parameters.");
            };

            // Begin main OSC update loop
            Utils.TimeBeginPeriod(1);
            while (!MasterCancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(10);

                if (_relevantParamsCount <= 0)
                    continue;

                UnifiedTracking.UpdateData();

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