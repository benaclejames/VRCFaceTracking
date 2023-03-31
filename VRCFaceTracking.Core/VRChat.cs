using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace VRCFaceTracking
{
    public static class VRChat
    {
        public static readonly string VRCData = Path.Combine(Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat\\VRChat");
        
        public static readonly string VRCOSCDirectory = Path.Combine(VRCData, "OSC");
        
        public static bool ForceEnableOsc()
        {
            // Set all registry keys containing osc in the name to 1 in Computer\HKEY_CURRENT_USER\Software\VRChat\VRChat
            var regKey = Registry.CurrentUser.OpenSubKey("Software\\VRChat\\VRChat", true);
            if (regKey == null)
                return true;    // Assume we already have osc enabled
            
            var keys = regKey.GetValueNames().Where(x => x.ToLower().Contains("osc"));

            bool wasOscForced = false;
            foreach (var key in keys)
            {
                if ((int) regKey.GetValue(key) == 0)
                {
                    // Osc is likely not enabled
                    regKey.SetValue(key, 1);
                    wasOscForced = true;
                }
            }

            return wasOscForced;
        }
        
        public static bool IsVRChatRunning() => Process.GetProcesses().Any(x => x.ProcessName == "VRChat");
    }
}