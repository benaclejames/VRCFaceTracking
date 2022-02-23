using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        private static OscMain _oscMain;
        
        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<OSCParams.BaseParam> parameters)
        {
            var paramList = new List<OscMessage>();
            foreach (var param in parameters) {
                paramList.Add(new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamType, param.ParamValue));
            }

            return paramList;
        }

        public static IEnumerable<OSCParams.BaseParam> RelevantParams;

        public static void Main(string[] args)
        {
            Utils.TimeBeginPeriod(1);
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");
            _oscMain = new OscMain("127.0.0.1", 9000, 9001);
            
            RelevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
            
            Console.CancelKeyPress += delegate {
                Utils.TimeEndPeriod(1);
                Logger.Msg("VRCFT Standalone Exiting!");
                UnifiedLibManager.Teardown();
            };

            ConfigParser.OnConfigLoaded += () =>
            {
                RelevantParams = UnifiedTrackingData.AllParameters.SelectMany(p => p.GetBase()).Where(param => param.Relevant);
                Logger.Msg("Config file parsed successfully! "+RelevantParams.Count()+" parameters loaded");
            };

            while (true)
            {
                Thread.Sleep(10);
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(RelevantParams));
                
                _oscMain.Send(bundle.Data);
            }
        }
    }
}