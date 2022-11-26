using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

  public class Globals
  {
    private static string _ip, _opMode;
    private static int _inPort, _outPort;
    public static string ip
    {
      get
      {
        return _ip;
      }
      set
      {
        _ip = value;
      }
    }
    public static string opMode
    {
      get
      {
        return _opMode;
      }
      set
      {
        _opMode = value;
      }
    }
    public static int inPort
    {
      get
      {
        return _inPort;
      }
      set
      {
        _inPort = value;
      }
    }
    public static int outPort
    {
      get
      {
        return _outPort;
      }
      set
      {
        _outPort = value;
      }
    }

  }
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

        private static string _ip = "127.0.0.1", _opMode = "auto";
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

            (_outPort, _ip, _inPort, _opMode) = ArgsHandler.HandleArgs();

            // if auto opMode is active (default behaviour), detect running app and start in correct mode 
            if (_opMode == "auto")
            {
              Logger.Msg("Operating mode auto detecting.");
              bool _noset1 = false;
              bool _noset2 = false;
              if (CVR.IsCVRRunning())
              {
                Logger.Msg("ChilloutVR running switching opMode to cvr");
                _opMode = "cvr";
              }
              else
              {
                _noset1 |= true;
              }
              if (VRChat.IsVRChatRunning())
              {
                Logger.Msg("VRChat running switching opMode to vrc");
                _opMode = "vrc";
              }
              else
              {
                _noset2 |= true;
              }

              if( _noset1&& _noset2)
              {
                Logger.Error(
                      "Neither VRChat or ChilloutVR detected as running.\n" +
                      "opMode falling back to vrc");
                _opMode = "vrc";
              }
            }

            Logger.Msg("InPort = " + _inPort + " , OutPort = " + _outPort + " , target IP = " + _ip);
            Logger.Msg("Operating Mode = " + _opMode);
            // setup global values
            Globals.inPort = _inPort;
            Globals.outPort = _outPort;
            Globals.ip = _ip;
            Globals.opMode = _opMode;


            // Load dependencies
            DependencyManager.Load();

            if (Globals.opMode == "vrc")
            {
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
              // Warn about mode if CVR detected running (as mode is currenty vrc)
              if (CVR.IsCVRRunning())
              {
                Logger.Error("Operating mode is currenly VRChat but ChilloutVR is currently running. \n " +
                  "you may want to set opMode to cvr");
              }
            }
            if (Globals.opMode == "cvr")
            {
              if (VRChat.IsVRChatRunning())
              {
                Logger.Error(
                    "Operating mode is currenly set to ChilloutVR but VRChat is currently running. \n " +
                    "you may want to set opMode to vrc");
              }
            }

      // Initialize Tracking Runtimes
      UnifiedLibManager.Initialize();

            // Initialize Locals
            OscMain = new OscMain();
            var bindResults = OscMain.Bind(_ip, _outPort, _inPort);
            if (!bindResults.receiverSuccess)
                Logger.Error("Socket failed to bind to receiver port " + _inPort + ", please ensure it's not already in use by another program or specify a different one instead.");
            
            if (!bindResults.senderSuccess)
                Logger.Error("Socket failed to bind to sender port "+ _outPort + ", please ensure it's not already in use by another program or specify a different one instead.");

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