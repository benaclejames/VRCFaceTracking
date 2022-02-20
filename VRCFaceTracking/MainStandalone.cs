using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

        public static extern uint TimeEndPeriod(uint uMilliseconds);

        public static void Main()
        {
            TimeBeginPeriod(1);
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
                    UnifiedTrackingData.LatestLipShapes);

                var bundle = new OscBundle(ConstructMessages(allParams));
                
                OSCMain.Send(bundle.Data);
            }

            TimeEndPeriod(1);
        }
    }
}