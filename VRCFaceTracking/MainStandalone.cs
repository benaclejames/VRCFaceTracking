using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        private static OscMain _oscMain;
        
        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<OSCParams.BaseParam> parameters) => 
            parameters.Select(param => new OscMessage(param.OutputInfo.address, param.OscType, param.ParamValue)).ToList();

        private static IEnumerable<OSCParams.BaseParam> _relevantParams;

        private static InputManager _inputManager;

        private static string _ip = "127.0.0.1";
        private static int _inPort = 9001, _outPort = 9000;
        
        public static void Main(string[] args)
        {
            // Parse Arguments
            foreach (var arg in args)
            {
                if (arg.StartsWith("--osc="))
                {
                    var oscConfig = arg.Remove(0, 6).Split(':');
                    if (oscConfig.Length < 3)
                    {
                        Console.WriteLine("Invalid OSC config: " + arg +"\nExpected format: --osc=<OutPort>:<IP>:<InPort>");
                        return;
                    }

                    if (!int.TryParse(oscConfig[0], out _outPort))
                    {
                        Console.WriteLine("Invalid OSC OutPort: " + oscConfig[0]);
                        return;
                    }
                    
                    if (!new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$").IsMatch(oscConfig[1]))
                    {
                        Console.WriteLine("Invalid OSC IP: " + oscConfig[1]);
                        return;
                    } 
                    _ip = oscConfig[1];
                    
                    if (!int.TryParse(oscConfig[2], out _inPort))
                    {
                        Console.WriteLine("Invalid OSC InPort: " + oscConfig[2]);
                        return;
                    }
                }
            }
            
            // Initialize dependencies and tracking runtimes
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");
            
            // Initialize Locals
            _oscMain = new OscMain(_ip, _outPort, _inPort);
            _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
            _inputManager = new InputManager();
            int paramCount = 0;
            
            // Bind callbacks
            Console.CancelKeyPress += delegate {
                Utils.TimeEndPeriod(1);
                Logger.Msg("VRCFT Standalone Exiting!");
                UnifiedLibManager.Teardown();
            };

            ConfigParser.OnConfigLoaded += () =>
            {
                _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
                UnifiedTrackingData.LatestEyeData.ResetThresholds();
                paramCount = _relevantParams.Count();
                Logger.Msg("Config file parsed successfully! "+paramCount+" parameters loaded");
            };

            Logger.Warning("Due to a bug with VRChat's current OSC implementation, it's important you pause the runtime before switching avatars, and unpause when the swap has completed.\nPress P to toggle the pause state.");
            
            // Begin main OSC update loop
            Utils.TimeBeginPeriod(1);
            while (true)
            {
                Thread.Sleep(10);
                // If RelevantParams is empty, or we're paused, don't update or send a bundle
                if (_inputManager.ShouldPause || paramCount == 0)
                    continue;
                
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(_relevantParams));
                _oscMain.Send(bundle.Data);
            }
        }
    }
}