using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
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
        private static List<Type> _allModules;
        private static List<Type> _requestedModules = null;
        private static ExtTrackingModule _loadedEyeModule;
        private static ExtTrackingModule _loadedExpressionModule;
        private static readonly Dictionary<ExtTrackingModule, Thread> UsefulThreads =
            new Dictionary<ExtTrackingModule, Thread>();
        #endregion
        
        private static Thread _initializeWorker;

        public static void Initialize()
        {
            if (_initializeWorker != null && _initializeWorker.IsAlive) _initializeWorker.Abort();
            
            // Start Initialization
            _initializeWorker = new Thread(() =>
            {
                // Kill lingering threads
                TeardownAllAndReset();

                // Load all available modules in CustomLibs
                _allModules = LoadExternalModules();

                // Attempt to initialize the requested runtime.
                InitRequestedRuntimes(_requestedModules);
            });
            Logger.Msg("Starting initialization thread");
            _initializeWorker.Start();
        }

        public static void LoadRequestedModules()
        {
            Initialize();
        }

        public static void RequestModules(List<Type> moduleTypes)
        {
            _requestedModules = moduleTypes;
        }

        public static List<Type> GetModuleList()
        {
            return _allModules;
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
                catch (BadImageFormatException e)
                {
                    Logger.Error("Encountered a .dll with an invalid format: " + e.Message+". Skipping...");
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

        private static void InitRequestedRuntimes(List<Type> moduleType)
        {
            Logger.Msg("Initializing runtimes...");

            if (moduleType == null)
            {
                Logger.Warning("Select a module under the 'Modules' tab and/or obtain a VRCFaceTracking tracking extension module.");
                return;
            }

            foreach (Type module in moduleType)
            {
                Logger.Msg("Initializing module: " + module.Name);
                // Create module
                var moduleObj = (ExtTrackingModule) Activator.CreateInstance(module);
                
                // Attempt to load up modules
                if (moduleObj.Supported.SupportsEye || moduleObj.Supported.SupportsExpressions)
                {
                    bool eyeSuccess, expressionSuccess;
                    try
                    {
                        (eyeSuccess, expressionSuccess) = moduleObj.Initialize();
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
                    
                    // If eyeSuccess is true, set the eye status to active and load the eye module slot.
                    if (eyeSuccess && _loadedEyeModule == null)
                    {
                        _loadedEyeModule = moduleObj;
                        EyeStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }

                    // If module is already loaded, initialize teardown and load up the requested module from the UI.
                    if (eyeSuccess && _loadedEyeModule != null)
                    {
                        _loadedEyeModule = moduleObj;
                        EyeStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }

                    // If expressionSuccess is true, set the eye status to active and load the expressions/s module slot.
                    if (expressionSuccess && _loadedExpressionModule == null)
                    {
                        _loadedExpressionModule = moduleObj;
                        ExpressionStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }

                    // If module is already loaded, initialize teardown and load up the requested module from the UI.
                    if (expressionSuccess && _loadedExpressionModule != null)
                    {
                        _loadedExpressionModule = moduleObj;
                        ExpressionStatus = ModuleState.Active;
                        EnsureModuleThreadStarted(moduleObj);
                    }
                }

                if (EyeStatus > ModuleState.Uninitialized && ExpressionStatus > ModuleState.Uninitialized) 
                    break;    // Keep enumerating over all modules until we find ones we can use
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