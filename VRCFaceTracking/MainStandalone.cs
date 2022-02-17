using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using ParamLib;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public class MainStandalone
    {
        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static IEnumerable<OscMessage> ConstructMessages(IEnumerable<BaseParam> parameters) => parameters.Select(param => new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));

        public static void Main()
        {
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");

            while (true)
            {
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                var thing = UnifiedTrackingData.AllParameters.SelectMany(param => param.GetBase().Where(b => b.GetType() == typeof(FloatParameter) || b.GetType() == typeof(FloatBaseParam)));

                var nextMessage = new OscBundle(ConstructMessages(thing));
                OSCMain.SendOscBundle(nextMessage);
            }
        }
    }
}