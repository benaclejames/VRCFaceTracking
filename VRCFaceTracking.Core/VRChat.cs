using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace VRCFaceTracking.Core;

public static class VRChat
{
    static VRChat()
    {
        if (OperatingSystem.IsWindows())
        {
            VRCOSCDirectory = Path.Combine(
                $"{Environment.GetEnvironmentVariable("localappdata")}Low", "VRChat", "VRChat", "OSC"
            );
        }

        if (OperatingSystem.IsLinux())
        {
            /* On macOS/Linux, things are a little different. The above points to a non-existent folder
             * Thankfully, we can make some assumptions based on the fact VRChat on Linux runs through Proton
             * For reference, here is what a target path looks like:
             * /home/USER_NAME/.steam/steam/steamapps/compatdata/438100/pfx/drive_c/users/steamuser/AppData/LocalLow/VRChat/VRChat/OSC/
             * Where 438100 is VRChat's Steam GameID, and the path after "steam" is pretty much fixed */

            // 1) First, get the user profile folder
            // (/home/USER_NAME/)
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // 2) Then, search for common Steam install paths
            // (/home/USER_NAME/.steam/steam/)
            string[] possiblePaths =
            {
                Path.Combine(home, ".steam", "steam"),
                Path.Combine(home, ".local", "share", "Steam"),
                Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam")
            };
            var steamPath = Array.Find(possiblePaths, Directory.Exists);

            if (string.IsNullOrEmpty(steamPath))
            {
                throw new InvalidProgramException("Steam was not detected!");
            }

            // 3) Inside the steam install directory, find the file steamPath/steamapps/libraryfolders.vdf
            // This is a special file that tells us where on a users computer their steam libraries are
            var steamLibrariesPath = Path.Combine(steamPath!, "steamapps", "libraryfolders.vdf");

            // Parse the VDF file manually instead of using Gameloop.Vdf
            var libraryFolders = ParseLibraryFoldersVdf(File.ReadAllText(steamLibrariesPath));

            var vrchatPath = string.Empty;
            foreach (var library in libraryFolders)
            {
                if (library.HasVRChat)
                {
                    vrchatPath = library.Path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(vrchatPath))
            {
                throw new InvalidProgramException(
                    "Steam was detected, but VRChat was not detected on this system! Is it installed?");
            }

            // 4) Finally, construct the path to the user's VRChat install
            VRCOSCDirectory = Path.Combine(vrchatPath, "steamapps", "compatdata", "438100", "pfx", "drive_c",
                "users", "steamuser", "AppData", "LocalLow", "VRChat", "VRChat", "OSC");
        }
    }

    public static string VRCOSCDirectory
    {
        get; private set;
    }

    /// <summary>
    /// Parse the Steam libraryfolders.vdf file to extract library paths and installed apps
    /// </summary>
    /// <param name="vdfContent">Content of the libraryfolders.vdf file</param>
    /// <returns>List of parsed library folders</returns>
    private static List<LibraryFolder> ParseLibraryFoldersVdf(string vdfContent)
    {
        var result = new List<LibraryFolder>();

        // Define regex patterns for key components
        var libraryBlockPattern = @"\""\d+\""[\s\n\r]*{(.*?)}";
        var pathPattern = @"\""path\""[\s\n\r]*\""(.*?)\""";
        var appsBlockPattern = @"\""apps\""[\s\n\r]*{(.*?)}";
        var appEntryPattern = @"\""(\d+)\""[\s\n\r]*\"".*?\""";

        // Find all library blocks
        var libraryMatches = Regex.Matches(vdfContent, libraryBlockPattern,
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match libraryMatch in libraryMatches)
        {
            var libraryContent = libraryMatch.Groups[1].Value;

            // Extract path
            var pathMatch = Regex.Match(libraryContent, pathPattern, RegexOptions.Singleline);
            if (!pathMatch.Success) continue;

            var libraryPath = pathMatch.Groups[1].Value.Replace(@"\\", @"\"); // Fix escaped backslashes

            // Extract apps block
            var appsMatch = Regex.Match(libraryContent, appsBlockPattern, RegexOptions.Singleline);
            var hasVRChat = false;

            if (appsMatch.Success)
            {
                var appsContent = appsMatch.Groups[1].Value;
                var appMatches = Regex.Matches(appsContent, appEntryPattern);

                foreach (Match appMatch in appMatches)
                {
                    var appId = appMatch.Groups[1].Value;
                    if (appId == "438100") // VRChat's AppID
                    {
                        hasVRChat = true;
                        break;
                    }
                }
            }

            result.Add(new LibraryFolder
            {
                Path = libraryPath,
                HasVRChat = hasVRChat
            });
        }

        return result;
    }

    /// <summary>
    /// Simple class to store Steam library folder information
    /// </summary>
    private class LibraryFolder
    {
        public string Path { get; set; } = string.Empty;
        public bool HasVRChat
        {
            get; set;
        }
    }

    [SupportedOSPlatform("windows")]
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
            if ((int)regKey.GetValue(key) == 0)
            {
                // Osc is likely not enabled
                regKey.SetValue(key, 1);
                wasOscForced = true;
            }
        }

        return wasOscForced;
    }

    public static bool IsVrChatRunning() => Process.GetProcesses().Any(x => x.ProcessName == "VRChat");
}
