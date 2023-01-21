using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace VRCFaceTracking
{
    public static class CVR
    {
        public static readonly string CVRData = Path.Combine(Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "Alpha Blend Interactive\\ChilloutVR");
        
        public static readonly string CVROSCDirectory = Path.Combine(CVRData, "OSC");
        
        public static bool IsCVRRunning() => Process.GetProcesses().Any(x => x.ProcessName == "ChilloutVR");
    }
}