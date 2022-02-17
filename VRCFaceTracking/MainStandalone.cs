using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using OSCTest;
using ParamLib;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Eye;
using VRCFaceTracking.Params.LipMerging;

namespace VRCFaceTracking
{
    public class MainStandalone
    {
        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static OscMessage[] ConstructMessages(BaseParam[] parameters) => parameters.Select(param => new OscMessage("/" + param.ParamName, param.ParamValue)).ToArray();

        public static void Main()
        {
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");

            while (true)
            {
                Thread.Sleep(10);
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                var thing = UnifiedTrackingData.AllParameters.SelectMany(param =>
                {
                    return param.GetBase().Where(b => b.GetType() == typeof(FloatParameter) || b.GetType() == typeof(FloatBaseParam));
                });
                
                var nextMessage = new OscBundle(ConstructMessages(thing.ToArray()));
                OSCMain.SendOSCBundle(nextMessage);
            }
        }
    }
}