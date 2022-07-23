using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace VRCFaceTracking
{
    public enum ModuleState
    {
        Uninitialized = -1, // If the module is not initialized, we can assume it's not being used
        Idle = 0,   // Idle and above we can assume the module in question is or has been in use
        Active = 1  // We're actively getting tracking data from the module
    }

    public static class UnifiedLibManager
    {
        #region Delegates
        public static Action<ModuleState, ModuleState> OnTrackingStateUpdate = (b, b1) => { };
        #endregion
        
        #region Statuses
        public static ModuleState EyeStatus
        {
            get => _eyeModule?.Status.EyeState ?? ModuleState.Uninitialized;
            set
            {
                if (_eyeModule != null)
                    _eyeModule.Status.EyeState = value;
                OnTrackingStateUpdate.Invoke(value, LipStatus);
            }
        }

        public static ModuleState LipStatus
        {
            get => _lipModule?.Status.LipState ?? ModuleState.Uninitialized;
            set
            {
                if (_lipModule != null)
                    _lipModule.Status.LipState = value;
                OnTrackingStateUpdate.Invoke(EyeStatus, value);
            }
        }
        #endregion

        #region Modules
        private static ExtTrackingModule _eyeModule, _lipModule;
        private static readonly Dictionary<ExtTrackingModule, Thread> UsefulThreads =
            new Dictionary<ExtTrackingModule, Thread>();
        #endregion
        
        private static Thread _initializeWorker;

        public static void Initialize(bool eye = true, bool lip = true)
        {
            // Kill lingering threads
            if (_initializeWorker != null && _initializeWorker.IsAlive) _initializeWorker.Abort();
            TeardownAllAndReset();

            // Start Initialization
            _initializeWorker = new Thread(() => FindAndInitRuntimes(eye, lip));
            _initializeWorker.Start();
        }

        private static List<Type> LoadExternalModules()
        {
            var returnList = new List<Type>();
            var customLibsPath = Path.Combine(Utils.PersistentDataDirectory, "CustomLibs");
            
            if (!Directory.Exists(customLibsPath))
                Directory.CreateDirectory(customLibsPath);
            
            Logger.Msg("Loading External Modules...");

            // Load dotnet dlls from the VRCFTLibs folder
            foreach (var dll in Directory.GetFiles(customLibsPath, "*.dll"))
            {
                Logger.Msg("Loading " + dll);

                Type module;
                try 
                {
                    var loadedModule = Assembly.LoadFrom(dll);
                    // Get the first class that implements ExtTrackingModule
                    module = loadedModule.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(ExtTrackingModule)));
                }
                catch (ReflectionTypeLoadException e)
                {
                    foreach (var loaderException in e.LoaderExceptions)
                    {
                        Logger.Error("LoaderException: " + loaderException.Message);
                    }
                    Logger.Error("Exception loading " + dll + ". Skipping.");
                    continue;
                }
                
                if (module != null)
                {
                    returnList.Add(module);
                    Logger.Msg("Loaded external tracking module: " + module.Name);
                    continue;
                }
                
                Logger.Warning("Module " + dll + " does not implement ExtTrackingModule");
            }

            return returnList;
        }

        private static void EnsureModuleThreadStarted(ExtTrackingModule module)
        {
            if (UsefulThreads.ContainsKey(module))
                return;
            
            var thread = new Thread(module.GetUpdateThreadFunc().Invoke);
            UsefulThreads.Add(module, thread);
            thread.Start();
        }

        private static void FindAndInitRuntimes(bool eye = true, bool lip = true)
        {
            Logger.Msg("Finding and initializing runtimes...");

            // Get a list of our own built-in modules
            var trackingModules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(ExtTrackingModule)));

            // Concat both our own modules and the external ones
            trackingModules = trackingModules.Union(LoadExternalModules());
            
            foreach (var module in trackingModules)
            {
                // Create module
                var moduleObj = (ExtTrackingModule) Activator.CreateInstance(module);
                
                // If there is still a need for a module with eye or lip tracking and this module supports the current need, try initialize it
                if (EyeStatus == ModuleState.Uninitialized && moduleObj.Supported.SupportsEye ||
                    LipStatus == ModuleState.Uninitialized && moduleObj.Supported.SupportsLip)
                {
                    bool eyeSuccess, lipSuccess;
                    try
                    {
                        (eyeSuccess, lipSuccess) = moduleObj.Initialize(eye, lip);
                    }
                    catch(MissingMethodException)
                    {
                        Logger.Error(moduleObj.GetType().Name+ " does not properly implement ExtTrackingModule. Skipping.");
                        continue;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Exception initializing " + moduleObj.GetType().Name + ". Skipping.");
                        Logger.Error(e.Message);
                        continue;
                    }
                    
                    // If eyeSuccess or lipSuccess was true, set the status to active
                    if (eyeSuccess && _eyeModule == null)
                    {
                        _eyeModule = moduleObj;
                        EyeStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }

                    if (lipSuccess && _lipModule == null)
                    {
                        _lipModule = moduleObj;
                        LipStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }
                }

                if (EyeStatus > ModuleState.Uninitialized && LipStatus > ModuleState.Uninitialized) 
                    break;    // Keep enumerating over all modules until we find ones we can use
            }

            if (eye)
            {
                if (EyeStatus != ModuleState.Uninitialized) Logger.Msg("Eye Tracking Initialized via " + _eyeModule);
                else Logger.Warning("Eye Tracking will be unavailable for this session.");
            }

            if (lip)
            {
                if (LipStatus != ModuleState.Uninitialized) Logger.Msg("Lip Tracking Initialized via " +  _lipModule);
                else Logger.Warning("Lip Tracking will be unavailable for this session.");
            }
        }

        // Signal all active modules to gracefully shut down their respective runtimes
        public static void TeardownAllAndReset()
        {
            EyeStatus = ModuleState.Uninitialized;
            LipStatus = ModuleState.Uninitialized;
            
            foreach (var module in UsefulThreads)
            {
                module.Key.Teardown();
                module.Value.Abort();
            }
            UsefulThreads.Clear();

            _eyeModule = null;
            _lipModule = null;
        }
    }
}