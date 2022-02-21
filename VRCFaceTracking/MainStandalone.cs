using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParamLib;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        private static readonly Dictionary<BaseParam, double> CachedValues = new Dictionary<BaseParam, double>();
        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<BaseParam> parameters)
        {
            var paramList = new List<OscMessage>();
            foreach (var param in parameters) {
                if (!CachedValues.ContainsKey(param))
                {
                    CachedValues.Add(param, param.ParamValue);
                    paramList.Add(new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));
                }

                if (CachedValues[param] != param.ParamValue)
                {
                    CachedValues[param] = param.ParamValue;
                    paramList.Add(new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));
                }
            }

            return paramList;
        }

        public static void Main()
        {
            Utils.TimeBeginPeriod(1);
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");

            var allParams = UnifiedTrackingData.AllParameters.SelectMany(param => param.GetBase().Where(b => b.GetType() == typeof(FloatParameter) || b.GetType() == typeof(FloatBaseParam)));
            
            Console.CancelKeyPress += delegate {
                Utils.TimeEndPeriod(1);
                Logger.Msg("VRCFT Standalone Exiting!");
                UnifiedLibManager.Teardown();
            };
            
            while (true)
            {
                Thread.Sleep(10);
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(allParams));
                
                OSCMain.Send(bundle.Data);
            }
        }
    }
}