using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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
            "SRanipal.SRanipal.dll"
        };

        public static void Load()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            
            var dllPaths = ExtractAssemblies(AssembliesToLoad);
            foreach (var path in dllPaths)
                LoadAssembly(path);
        }
        
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs e)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            var assemblyName = new AssemblyName(e.Name);
            var dllName = assemblyName.Name + ".dll";

            // Load from Embedded Resources - This function is not called if the Assembly is already
            // in the same folder as the app.
            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(dllName));
            if (resources.Any())
            {

                // 99% of cases will only have one matching item, but if you don't,
                // you will have to change the logic to handle those cases.
                var resourceName = resources.First();
                using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    var block = new byte[stream.Length];

                    // Safely try to load the assembly.
                    try
                    {
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                    catch (IOException)
                    {
                        return null;
                    }
                    catch(BadImageFormatException)
                    {
                        return null;
                    }
                }
            }

            // in the case the resource doesn't exist, return null.
            return null;
        }

        private static IEnumerable<string> ExtractAssemblies(IEnumerable<string> resourceNames)
        {
            var extractedPaths = new List<string>();

            var dirName = Path.Combine(Utils.PersistentDataDirectory, "StockLibs");
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
                        Logger.Error("Failed to get DLL: " + e.Message);
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
                Logger.Error("Unable to load library " + path);
            else
                Logger.Msg("Loaded library " + path);
        }
    }
}