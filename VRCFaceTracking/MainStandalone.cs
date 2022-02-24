using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        private static OscMain _oscMain;
        
        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<OSCParams.BaseParam> parameters) => 
            parameters.Select(param => new OscMessage(param.OutputInfo.address, param.OscType, param.ParamValue)).ToList();

        private static IEnumerable<OSCParams.BaseParam> _relevantParams;

        public static void Main(string[] args)
        {
            Utils.TimeBeginPeriod(1);
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");
            _oscMain = new OscMain("127.0.0.1", 9000, 9001);
            
            _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
            
            Console.CancelKeyPress += delegate {
                Utils.TimeEndPeriod(1);
                Logger.Msg("VRCFT Standalone Exiting!");
                UnifiedLibManager.Teardown();
            };

            ConfigParser.OnConfigLoaded += () =>
            {
                _relevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
                Logger.Msg("Config file parsed successfully! "+_relevantParams.Count()+" parameters loaded");
            };

            while (true)
            {
                Thread.Sleep(50);
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(_relevantParams));
                
                _oscMain.Send(bundle.Data);
            }
        }
    }
}