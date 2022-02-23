using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace VRCFaceTracking
{
    public interface ITrackingModule
    {
        bool SupportsEye { get; }
        bool SupportsLip { get; }

        (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip);
        Action GetUpdateThreadFunc();
        void Update();
        void Teardown();
    }

    public static class UnifiedLibManager
    {
        public static bool EyeEnabled, LipEnabled;
        private static readonly Dictionary<Type, ITrackingModule> UsefulModules = new Dictionary<Type, ITrackingModule>();
        private static readonly List<Thread> UsefulThreads = new List<Thread>();
        
        private static Thread _initializeWorker;
        public static readonly bool ShouldThread = !Environment.GetCommandLineArgs().Contains("--vrcft-nothread");

        // Used when re-initializing modules, kills malfunctioning SRanipal process and restarts it. 
        public static IEnumerator CheckRuntimeSanity()
        {
            // Check we have UAC admin
            if (!Utils.HasAdmin)
            {
                Logger.Error("VRCFaceTracking must be running with Administrator privileges to force module reinitialization.");
                yield return null;
            }
            
            Logger.Msg("Checking Runtime Sanity...");
            EyeEnabled = false;
            LipEnabled = false;
            UsefulModules.Clear();
            foreach (var process in Process.GetProcessesByName("sr_runtime"))
            {
                Logger.Msg("Killing "+process.ProcessName);
                process.Kill();
                Thread.Sleep(3000);
                Logger.Msg("Re-Initializing");
                Initialize();
            }
        }
        
        public static void Initialize(bool eye = true, bool lip = true)
        {
            // Kill lingering threads
            if (_initializeWorker != null && _initializeWorker.IsAlive) _initializeWorker.Abort();
            foreach (var updateThread in UsefulThreads)
            {
                updateThread.Abort();
                UsefulThreads.Remove(updateThread);
            }
            
            // Start Initialization
            _initializeWorker = new Thread(() => FindAndInitRuntimes(eye, lip));
            _initializeWorker.Start();
        }

        private static List<Type> LoadExternalModules()
        {
            var returnList = new List<Type>();
            var path = Path.Combine(Utils.DataDirectory, "CustomLibs");
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return returnList;
            }
            
            Logger.Msg("Loading External Modules...");

            // Load dotnet dlls from the VRCFTLibs folder
            var dlls = Directory.GetFiles(path, "*.dll");
            foreach (var dll in dlls)
            {
                var loadedModule = Assembly.LoadFrom(Utils.DataDirectory+"\\"+dll);

                // Get the first type that implements ITrackingModule
                var module = loadedModule.GetTypes()
                    .FirstOrDefault(t => t.GetInterfaces().Contains(typeof(ITrackingModule)));
                if (module != null)
                {
                    returnList.Add(module);
                    Logger.Msg("Loaded external tracking module: " + module.Name);
                    continue;
                }
                
                Logger.Warning("Module " + dll + " does not implement ITrackingModule");
            }

            return returnList;
        }

        private static void FindAndInitRuntimes(bool eye = true, bool lip = true)
        {
            Logger.Msg("Finding and initializing runtimes...");

            var trackingModules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.GetInterfaces().Contains(typeof(ITrackingModule)));

            trackingModules = trackingModules.Union(LoadExternalModules());

            foreach (var module in trackingModules)
            {
                bool eyeSuccess = false, lipSuccess = false;    // Init result for every 
                
                var moduleObj = (ITrackingModule) Activator.CreateInstance(module);
                // If there is still a need for a module with eye or lip tracking and this module supports the current need, try initialize it
                if (!EyeEnabled && moduleObj.SupportsEye || !LipEnabled && moduleObj.SupportsLip)
                    (eyeSuccess, lipSuccess) = moduleObj.Initialize(eye, lip);

                // If the module successfully initialized anything, add it to the list of useful modules and start its update thread
                if ((eyeSuccess || lipSuccess) && !UsefulModules.ContainsKey(module))
                {
                    UsefulModules.Add(module, moduleObj);
                    if (ShouldThread)
                    {
                        Action updater = moduleObj.GetUpdateThreadFunc();
                        var updateThread = new Thread(() =>
                        {
                            updater.Invoke();
                        });
                        UsefulThreads.Add(updateThread);
                        updateThread.Start();
                    }
                }

                if (eyeSuccess) EyeEnabled = true;
                if (lipSuccess) LipEnabled = true;

                if (EyeEnabled && LipEnabled) break;    // Keep enumerating over all modules until we find ones we can use
            }

            if (eye)
            {
                if (EyeEnabled) Logger.Msg("Eye Tracking Initialized");
                else Logger.Warning("Eye Tracking will be unavailable for this session.");
            }

            if (lip)
            {
                if (LipEnabled) Logger.Msg("Lip Tracking Initialized");
                else Logger.Warning("Lip Tracking will be unavailable for this session.");
            }
        }

        // Signal all active modules to gracefully shut down their respective runtimes
        public static void Teardown()
        {
            foreach (var module in UsefulModules)
                module.Value.Teardown();
        }
        
        // Manually signal all useful modules to get the latest data
        public static void Update()
        {
            if (ShouldThread || !(EyeEnabled || LipEnabled)) return;
            
            foreach (var module in UsefulModules.Values)
                module.Update();
        }
    }
}