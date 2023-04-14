using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Library;

public enum ModuleState
{
    Uninitialized = -1, // If the module is not initialized, we can assume it's not being used
    Idle = 0,   // Idle and above we can assume the module in question is or has been in use
    Active = 1  // We're actively getting tracking data from the module
}

public class UnifiedLibManager : ILibManager
{
    #region Delegates
    public static Action<ModuleState, ModuleState> OnTrackingStateUpdate = (b, b1) => { };

    private static ILogger _logger;
    private static ILoggerFactory _loggerFactory;
    
    public ObservableCollection<ModuleMetadata> Modules
    {
        get;
        set;
    }

    public UnifiedLibManager(ILoggerFactory factory)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger("UnifiedLibManager");
        Modules = new ObservableCollection<ModuleMetadata>();
    }

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
    public static List<Assembly> AvailableModules { get; private set; }
    internal static List<Assembly> RequestedModules = new();
    private static ExtTrackingModule _loadedEyeModule, _loadedExpressionModule;
    private static readonly Dictionary<ExtTrackingModule, CancellationTokenSource> UsefulThreads = new();
    private static bool _enableTracking = true;
    #endregion

    private static Thread _initializeWorker;
    private static CancellationTokenSource _initCts;

    private void CreateModuleInitializer(List<Assembly> modules)
    {
        if (_initializeWorker != null && _initializeWorker.IsAlive && _initCts != null) _initCts.Cancel();

        // Start Initialization
        _initializeWorker = new Thread(() =>
        {
            // Kill lingering threads
            TeardownAllAndReset();

            // Attempt to initialize the requested runtimes.
            if (modules != null)
                InitRequestedRuntimes(modules);
            else _logger.LogWarning("Select a module under the 'Modules' tab and/or obtain a VRCFaceTracking tracking extension module.");

        });
        _logger.LogInformation("Starting initialization tracking");
        _initializeWorker.Start();
    }

    public void Initialize()
    {
        if (RequestedModules != null && RequestedModules.Count > 0)
            CreateModuleInitializer(RequestedModules);
        else CreateModuleInitializer(AvailableModules);
    }

    public static void SetTrackingEnabled(bool newEnabled)
    {
        _enableTracking = newEnabled;
    }

    public static void ReloadModules()
    {
        AvailableModules = LoadExternalAssemblies(GetAllModulePaths());
    }

    private static string[] GetAllModulePaths()
    {
        List<string> modulePaths = new List<string>();

        /*
        string customLibsExe = "CustomLibs";

        if (Directory.Exists(customLibsExe))
            modulePaths.AddRange(Directory.GetFiles(customLibsExe, "*.dll"));
        */

        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);

        modulePaths.AddRange(Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll"));

        return modulePaths.ToArray();
    }

    private static ExtTrackingModule LoadExternalModule(Assembly dll)
    {
        _logger.LogInformation("Loading External Module " + dll.FullName);

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
                _logger.LogError("LoaderException: " + loaderException.Message);
            }
            _logger.LogError("Exception loading " + dll + ". Skipping.");
        }
        catch (BadImageFormatException e)
        {
            _logger.LogError("Encountered a .dll with an invalid format: " + e.Message + ". Skipping...");
        }
        catch (TypeLoadException)
        {
            _logger.LogWarning("Module " + dll + " does not implement ExtTrackingModule.");
        }

        return null;
    }

    public static List<Assembly> LoadExternalAssemblies(string[] path, bool useAttributes = true)
    {
        var returnList = new List<Assembly>();
        foreach (var dll in path)
        {
            try
            {
                Assembly loaded = Assembly.LoadFrom(dll);
                foreach(Type type in loaded.GetExportedTypes())
                    if (type.BaseType == typeof(ExtTrackingModule))
                    {
                        _logger.LogInformation(type.ToString() + " implements ExtTrackingModule.");
                        returnList.Add(loaded);
                        continue;
                    }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message + " Assembly not able to be loaded. Skipping.");
                continue;
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
                _logger.LogError(e.Message);
            }
        }

        return returnList;
    }

    private static void EnsureModuleThreadStarted(ExtTrackingModule module)
    {
        if (UsefulThreads.ContainsKey(module))
            return;

        var cts = new CancellationTokenSource();
        ThreadPool.QueueUserWorkItem(state =>
        {
            var token = (CancellationToken)state;
            while(!token.IsCancellationRequested)
            {
                if (_enableTracking)
                {
                    module.Update();
                }
            }
        }, cts.Token);
        UsefulThreads.Add(module, cts);
    }

    private static void AttemptModuleInitialize(ExtTrackingModule module)
    {
        if (!module.Supported.SupportsEye && !module.Supported.SupportsExpression)
        {
            return;
        }

        module.Logger = _loggerFactory.CreateLogger(module.GetType().Name);

        bool eyeSuccess = false, expressionSuccess = false;
        try
        {
            (eyeSuccess, expressionSuccess) = module.Initialize(_loadedEyeModule == null, _loadedExpressionModule == null);
        }
        catch (MissingMethodException)
        {
            _logger.LogError(module.GetType().Name + " does not properly implement ExtTrackingModule. Skipping.");
        }
        catch (Exception e)
        {
            _logger.LogError("Exception initializing " + module.GetType().Name + ". Skipping.");
            _logger.LogError(e.Message);
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

    private void InitRequestedRuntimes(List<Assembly> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach (Assembly module in moduleType)
        {
            if (_loadedEyeModule != null && _loadedExpressionModule != null)
                break;

            _logger.LogInformation("Initializing module: " + module.ToString());
            ExtTrackingModule loadedModule = LoadExternalModule(module);
            AttemptModuleInitialize(loadedModule);
            MainStandalone.DispatcherRun.Invoke(() => Modules.Add(loadedModule.ModuleInformation));
        }

        if (EyeStatus != ModuleState.Uninitialized) _logger.LogInformation("Eye Tracking Initialized via " + _loadedEyeModule);
        else _logger.LogWarning("Eye Tracking will be unavailable for this session.");

        if (ExpressionStatus != ModuleState.Uninitialized) _logger.LogInformation("Expression Tracking Initialized via " +  _loadedExpressionModule);
        else _logger.LogWarning("Expression Tracking will be unavailable for this session.");
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public static void TeardownAllAndReset()
    {
        foreach (var module in UsefulThreads)
        {
            _logger.LogInformation("Teardown: " + module.Key.GetType().Name);
            module.Key.Teardown();
            module.Value.Cancel();
            _logger.LogInformation("Teardown complete: " + module.Key.GetType().Name);
        }
        UsefulThreads.Clear();
        
        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;

        _loadedEyeModule = null;
        _loadedExpressionModule = null;
    }
}