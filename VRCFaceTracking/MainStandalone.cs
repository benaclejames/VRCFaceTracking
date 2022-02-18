using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using ParamLib;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<BaseParam> parameters) => parameters.Select(param => new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));

        public static void Main()
        {
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");

            var allParams = UnifiedTrackingData.AllParameters.SelectMany(param => param.GetBase().Where(b => b.GetType() == typeof(FloatParameter) || b.GetType() == typeof(FloatBaseParam)));
            while (true)
            {
                Thread.Sleep(10);
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(allParams));
                OSCMain.Send(bundle.Data);
            }
        }
    }
}