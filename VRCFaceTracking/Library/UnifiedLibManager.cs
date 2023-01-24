using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Documents;

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
            get => _loadedEyeModule?.Status.EyeState ?? ModuleState.Uninitialized;
            set
            {
                if (_loadedEyeModule != null)
                    _loadedEyeModule.Status.EyeState = value;
                OnTrackingStateUpdate.Invoke(value, ExpressionStatus);
            }
        }

        public static ModuleState ExpressionStatus
        {
            get => _loadedExpressionModule?.Status.ExpressionState ?? ModuleState.Uninitialized;
            set
            {
                if (_loadedExpressionModule != null)
                    _loadedExpressionModule.Status.ExpressionState = value;
                OnTrackingStateUpdate.Invoke(EyeStatus, value);
            }
        }
        #endregion

        #region Modules
        private static List<Assembly> _availableModules;
        internal static List<Assembly> _requestedModules = new List<Assembly>();
        private static ExtTrackingModule _loadedEyeModule, _loadedExpressionModule;
        private static readonly Dictionary<ExtTrackingModule, Thread> UsefulThreads =
            new Dictionary<ExtTrackingModule, Thread>();
        #endregion
        
        private static Thread _initializeWorker;

        private static void CreateModuleInitializer(List<Assembly> modules)
        {
            if (_initializeWorker != null && _initializeWorker.IsAlive) _initializeWorker.Abort();

            // Start Initialization
            _initializeWorker = new Thread(() =>
            {
                // Kill lingering threads
                TeardownAllAndReset();

                // Attempt to initialize the requested runtimes.
                if (modules != null)
                    InitRequestedRuntimes(modules);
                else Logger.Warning("Select a module under the 'Modules' tab and/or obtain a VRCFaceTracking tracking extension module.");

            });
            Logger.Msg("Starting initialization thread");
            _initializeWorker.Start();
        }

        public static void Initialize()
        {
            ReloadModules();

            if (_requestedModules != null && _requestedModules.Count > 0)
                CreateModuleInitializer(_requestedModules);
            else CreateModuleInitializer(_availableModules);
        }

        public static void ReloadModules()
        {
            _availableModules = LoadExternalAssemblies(GetAllModulePaths());
        }

        public static List<Assembly> GetModuleList()
        {
            return _availableModules;
        }

        private static string[] GetAllModulePaths()
        {
            List<string> modulePaths = new List<string>();
            string customLibsExe = "CustomLibs";

            if (Directory.Exists(customLibsExe))
                modulePaths.AddRange(Directory.GetFiles(customLibsExe, "*.dll"));

            if (!Directory.Exists(Utils.CustomLibsDirectory))
                Directory.CreateDirectory(Utils.CustomLibsDirectory);

            modulePaths.AddRange(Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll"));

            return modulePaths.ToArray();
        }

        private static ExtTrackingModule LoadExternalModule(Assembly dll)
        {
            Logger.Msg("Loading External Module " + dll.FullName);

            Type module;
            ExtTrackingModule moduleObj;
            try
            {
                // Get the first class that implements ExtTrackingModule
                module = dll.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(ExtTrackingModule)));
                moduleObj = (ExtTrackingModule)Activator.CreateInstance(module);

                return moduleObj;
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var loaderException in e.LoaderExceptions)
                {
                    Logger.Error("LoaderException: " + loaderException.Message);
                }
                Logger.Error("Exception loading " + dll + ". Skipping.");
            }
            catch (BadImageFormatException e)
            {
                Logger.Error("Encountered a .dll with an invalid format: " + e.Message + ". Skipping...");
            }
            catch (TypeLoadException)
            {
                Logger.Warning("Module " + dll + " does not implement ExtTrackingModule.");
            }

            return null;
        }

        public static List<Assembly> LoadExternalAssemblies(string[] path, bool useAttributes = true)
        {
            var returnList = new List<Assembly>();

            // Load dotnet dlls from the VRCFTLibs folder, and CustomLibs if it happens to be beside the EXE (for portability).
            foreach (var dll in path)
            {
                try
                {
                    Assembly loaded = Assembly.LoadFrom(dll);
                    foreach(Type type in loaded.GetExportedTypes())
                        if (type.BaseType == typeof(ExtTrackingModule))
                        {
                            Logger.Msg(type.ToString() + " implements ExtTrackingModule.");
                            returnList.Add(loaded);
                            continue;
                        }
                }
                catch (FileNotFoundException)
                {
                    Logger.Warning(dll + " failed to find file. Skipping.");
                }
                catch (ArgumentNullException)
                {
                    Logger.Warning(dll + " Assembly mismatch. Skipping.");
                }
            }
            if (useAttributes) 
            {
                try
                {
                    ModuleAttributeHandler.HandleModuleAttributes(ref returnList);
                }
                catch(Exception e)
                {
                    Logger.Error(e.Message);
                }
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

        private static void AttemptModuleInitialize(ExtTrackingModule module)
        {
            if (module.Supported.SupportsEye || module.Supported.SupportsExpressions)
            {
                bool eyeSuccess = false;
                bool expressionSuccess = false;
                try
                {
                    (eyeSuccess, expressionSuccess) = module.Initialize(_loadedEyeModule == null, _loadedExpressionModule == null);
                }
                catch (MissingMethodException)
                {
                    Logger.Error(module.GetType().Name + " does not properly implement ExtTrackingModule. Skipping.");
                }
                catch (Exception e)
                {
                    Logger.Error("Exception initializing " + module.GetType().Name + ". Skipping.");
                    Logger.Error(e.Message);
                }

                // If eyeSuccess is true, set the eye status to active and load the eye module slot. Overlapping eye modules won't be loaded.
                if (eyeSuccess && _loadedEyeModule == null)
                {
                    _loadedEyeModule = module;
                    EyeStatus = ModuleState.Active;
                    EnsureModuleThreadStarted(module);
                }

                // If expressionSuccess is true, set the eye status to active and load the expressions/s module slot. Overlapping expression modules won't be loaded (may change in the future).
                if (expressionSuccess && _loadedExpressionModule == null)
                {
                    _loadedExpressionModule = module;
                    ExpressionStatus = ModuleState.Active;
                    EnsureModuleThreadStarted(module);
                }
            }
        }

        private static void InitRequestedRuntimes(List<Assembly> moduleType)
        {
            Logger.Msg("Initializing runtimes...");

            foreach (Assembly module in moduleType)
            {
                ExtTrackingModule loadedModule;

                loadedModule = LoadExternalModule(module);
                Logger.Msg("Initializing module: " + module.ToString());
                AttemptModuleInitialize(loadedModule);
            }

            if (EyeStatus != ModuleState.Uninitialized) Logger.Msg("Eye Tracking Initialized via " + _loadedEyeModule);
            else Logger.Warning("Eye Tracking will be unavailable for this session.");

            if (ExpressionStatus != ModuleState.Uninitialized) Logger.Msg("Expression Tracking Initialized via " +  _loadedExpressionModule);
            else Logger.Warning("Expression Tracking will be unavailable for this session.");
        }

        // Signal all active modules to gracefully shut down their respective runtimes
        public static void TeardownAllAndReset()
        {
            foreach (var module in UsefulThreads)
            {
                Logger.Msg("Teardown: " + module.Key.GetType().Name);
                module.Key.Teardown();
                module.Value.Abort();
                Logger.Msg("Teardown complete: " + module.Key.GetType().Name);
            }
            UsefulThreads.Clear();
            
            EyeStatus = ModuleState.Uninitialized;
            ExpressionStatus = ModuleState.Uninitialized;

            _loadedEyeModule = null;
            _loadedExpressionModule = null;
        }
    }
}