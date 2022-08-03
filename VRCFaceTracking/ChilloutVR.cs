using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VRCFaceTracking
{
    public class ChilloutVR
    {
        public static readonly string CVRData = Path.Combine(Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "Alpha Blend Interactive\\ChilloutVR\\");
    
        public static readonly string CCVROSCDirectory = Path.Combine(CVRData, "OSC");
    
        public static bool IsChilloutVRRunning() => Process.GetProcesses().Any(x => x.ProcessName == "ChilloutVR");
    }
}