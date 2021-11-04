using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

namespace VRCFaceTracking
{
    public static class DependencyManager
    {
        // Because SRanipal.dll needs to be loaded last.. Too lazy to automate moving it to back of load queue
        private static readonly string[] AssembliesToLoad = {
            "SRanipal.libHTC_License.dll",
            "SRanipal.nanomsg.dll",
            "SRanipal.SRWorks_Log.dll",
            "SRanipal.ViveSR_Client.dll",
            "SRanipal.SRanipal.dll",
            "Pimax.PimaxEyeTracker.dll"
        };

        public static void Init()
        {
            var dllPaths = ExtractAssemblies(AssembliesToLoad);
            foreach (var path in dllPaths)
                LoadAssembly(path);
        }

        private static IEnumerable<string> ExtractAssemblies(IEnumerable<string> resourceNames)
        {
            var extractedPaths = new List<string>();

            var melonInfo = Assembly.GetExecutingAssembly().CustomAttributes.ToList()
                .Find(a => a.AttributeType == typeof(MelonInfoAttribute));
            
            var dirName = Path.Combine(Path.GetTempPath(), melonInfo.ConstructorArguments[1].Value.ToString());
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            foreach (var dll in resourceNames)
            {
                var dllPath = Path.Combine(dirName, GetAssemblyNameFromPath(dll));

                using (var stm = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCFaceTracking.TrackingLibs."+dll))
                {
                    try
                    {
                        using (Stream outFile = File.Create(dllPath))
                        {
                            const int sz = 4096;
                            var buf = new byte[sz];
                            while (true)
                            {
                                if (stm == null) continue;
                                var nRead = stm.Read(buf, 0, sz);
                                if (nRead < 1)
                                    break;
                                outFile.Write(buf, 0, nRead);
                            }
                        }

                        extractedPaths.Add(dllPath);
                    }
                    catch(Exception e)
                    {
                        MelonLogger.Error($"Failed to get DLL: " + e.Message);
                    }
                }
            }
            return extractedPaths; 
        }

        private static string GetAssemblyNameFromPath(string path)
        {
            var splitPath = path.Split('.').ToList();
            splitPath.Reverse();
            return splitPath[1]+".dll";
        }
        
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private static void LoadAssembly(string path)
        {
            if (LoadLibrary(path) == IntPtr.Zero)
                MelonLogger.Error("Unable to load library " + path);
        }
    }
}