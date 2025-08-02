using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace VRCFaceTracking.Core;

public static class VRChat
{
    public static void EnsureVRCOSCDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            VRCOSCDirectory = Path.Combine(
                $"{Environment.GetEnvironmentVariable("localappdata")}Low", "VRChat", "VRChat", "OSC"
            );
        }
        else
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

            // Parse the VDF file without using Gameloop.Vdf
            var libraryFolders = ParseVdfFile(File.ReadAllText(steamLibrariesPath));

            // From libraryFolders, find the one containing VRChat
            var vrchatPath = string.Empty;

            // libraryFolders should have a root "libraryfolders" dictionary
            if (libraryFolders.TryGetValue("libraryfolders", out var libraryFoldersDict) &&
                libraryFoldersDict is Dictionary<string, object> libraries)
            {
                // Each library is indexed by a number (0, 1, 2, etc.)
                foreach (var library in libraries)
                {
                    if (library.Value is Dictionary<string, object> libraryData &&
                        libraryData.TryGetValue("path", out var pathObj) &&
                        libraryData.TryGetValue("apps", out var appsObj))
                    {
                        string libraryPath = pathObj.ToString();

                        // Check if VRChat is in the apps dictionary
                        if (appsObj is Dictionary<string, object> apps && apps.ContainsKey("438100"))
                        {
                            vrchatPath = libraryPath;
                            break;
                        }
                    }
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

    public static string VRCOSCDirectory { get; private set; }

    /// <summary>
    /// Parse a VDF file into a dictionary structure
    /// </summary>
    /// <param name="vdfContent">Content of the VDF file</param>
    /// <returns>Dictionary representing the VDF structure</returns>
    private static Dictionary<string, object> ParseVdfFile(string vdfContent)
    {
        var parser = new VdfParser(vdfContent);
        return parser.Parse();
    }

    /// <summary>
    /// A simple parser for Valve's VDF format
    /// </summary>
    private class VdfParser
    {
        private readonly string _content;
        private int _position;

        public VdfParser(string content)
        {
            _content = content;
            _position = 0;
        }

        public Dictionary<string, object> Parse()
        {
            SkipWhitespace();
            return ParseObject();
        }

        private Dictionary<string, object> ParseObject()
        {
            var result = new Dictionary<string, object>();

            while (_position < _content.Length)
            {
                SkipWhitespace();

                // Check for end of object
                if (_position < _content.Length && _content[_position] == '}')
                {
                    _position++; // Skip the closing brace
                    break;
                }

                // Parse key
                string key = ParseString();
                if (string.IsNullOrEmpty(key))
                    break;

                SkipWhitespace();

                // Parse value
                object value;

                // Check if the value is an object
                if (_position < _content.Length && _content[_position] == '{')
                {
                    _position++; // Skip the opening brace
                    value = ParseObject();
                }
                else
                {
                    value = ParseString();
                }

                result[key] = value;
            }

            return result;
        }

        private string ParseString()
        {
            SkipWhitespace();

            // Check for quoted string
            if (_position < _content.Length && _content[_position] == '"')
            {
                _position++; // Skip opening quote

                var startPos = _position;

                // Find the closing quote
                while (_position < _content.Length && _content[_position] != '"')
                {
                    // Handle escaped characters
                    if (_content[_position] == '\\' && _position + 1 < _content.Length)
                    {
                        _position += 2; // Skip the escape sequence
                    }
                    else
                    {
                        _position++;
                    }
                }

                if (_position < _content.Length)
                {
                    var result = _content.Substring(startPos, _position - startPos);
                    _position++; // Skip closing quote
                    return result;
                }
            }

            return string.Empty;
        }

        private void SkipWhitespace()
        {
            while (_position < _content.Length)
            {
                char c = _content[_position];

                if (char.IsWhiteSpace(c) || c == '\r' || c == '\n' || c == '\t')
                {
                    _position++;
                }
                else if (c == '/' && _position + 1 < _content.Length && _content[_position + 1] == '/')
                {
                    // Skip single line comments
                    _position += 2;
                    while (_position < _content.Length && _content[_position] != '\n')
                    {
                        _position++;
                    }
                }
                else
                {
                    break;
                }
            }
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
            if ((int) regKey.GetValue(key) == 0)
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
