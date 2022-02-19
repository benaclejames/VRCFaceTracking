using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Win32;
using ParamLib;
using VRCFaceTracking.OSC;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public static class MainStandalone
    {
        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private static Dictionary<BaseParam, double> cachedValues = new Dictionary<BaseParam, double>();
        private static IEnumerable<OscMessage> ConstructMessages(IEnumerable<BaseParam> parameters)
        {
            var paramList = new List<OscMessage>();
            foreach (var param in parameters) {
                if (!cachedValues.ContainsKey(param))
                {
                    cachedValues.Add(param, param.ParamValue);
                    paramList.Add(new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));
                }

                if (cachedValues[param] != param.ParamValue)
                {
                    cachedValues[param] = param.ParamValue;
                    paramList.Add(new OscMessage("/avatar/parameters/" + param.ParamName, param.ParamValue));
                }
            }

            return paramList;
        }

        public static void Main()
        {
            bool isMono = Type.GetType("Mono.Runtime") != null;
            if (!isMono)
            {
                string monoPath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Mono", "SdkInstallRoot", null);
                if (!string.IsNullOrEmpty(monoPath))
                {
                    Logger.Msg("Restarting in Mono");
                    
                    // Run the program with Mono
                    System.Diagnostics.Process.Start(monoPath+"\\bin\\mono.exe",System.Reflection.Assembly.GetExecutingAssembly().Location);
                    return;
                }
                
                Logger.Error("Currently running in non-mono mode. While this will work, it is not recommended and may cause some parameter values to be laggy.\n" +
                             "Please install Mono and restart the program.");
            }
            
            Logger.Msg("VRCFT Standalone Initializing!");
            DependencyManager.Init();
            Logger.Msg("Initialized DependencyManager Successfully");
            UnifiedLibManager.Initialize();
            Logger.Msg("Initialized UnifiedLibManager Successfully");

            var allParams = UnifiedTrackingData.AllParameters.SelectMany(param => param.GetBase().Where(b => b.GetType() == typeof(FloatParameter) || b.GetType() == typeof(FloatBaseParam)));
            while (true)
            {
                UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData,
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(allParams));
                
                OSCMain.Send(bundle.Data);
            }
        }
    }
}