using System.Diagnostics;
using Microsoft.Win32;

namespace VRCFaceTracking.Core
{
    public static class VRChat
    {
        public static readonly string VRCData = Path.Combine($"{Environment.GetEnvironmentVariable("localappdata")}Low", "VRChat\\VRChat");
        
        public static readonly string VRCOSCDirectory = Path.Combine(VRCData, "OSC");
        
        public static bool ForceEnableOsc()
        {
            // Set all registry keys containing osc in the name to 1 in Computer\HKEY_CURRENT_USER\Software\VRChat\VRChat
            var regKey = Registry.CurrentUser.OpenSubKey("Software\\VRChat\\VRChat", true);
            if (regKey == null)
                return true;    // Assume we already have osc enabled
            
            var keys = regKey.GetValueNames().Where(x => x.StartsWith("VRC_INPUT_OSC") || x.StartsWith("UI.Settings.Osc"));

            var wasOscForced = false;
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