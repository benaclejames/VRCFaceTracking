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
    #region Logger
    private static ILogger _logger;
    private static ILoggerFactory _loggerFactory;
    #endregion

    #region Observables
    public ObservableCollection<ModuleMetadata> ModuleMetadatas { get; set; }
    private readonly IDispatcherService _dispatcherService;
    #endregion

    #region Statuses
    public static ModuleState EyeStatus { get; private set; }
    public static ModuleState ExpressionStatus { get; private set; }
    #endregion

    #region Modules
    private struct ModuleThread
    {
        public ExtTrackingModule module;
        public CancellationTokenSource token;
    }
    public static List<Assembly> AvailableModules { get; private set; }
    private static readonly List<ModuleThread> ModuleThreads = new();
    #endregion

    #region Thread
    private static Thread _initializeWorker;
    private static readonly CancellationTokenSource _initCts;
    #endregion

    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger("UnifiedLibManager");
        _dispatcherService = dispatcherService;

        ModuleMetadatas = new ObservableCollection<ModuleMetadata>();
    }

    public void Initialize()
    {
        if (_initializeWorker != null && _initializeWorker.IsAlive && _initCts != null) _initCts.Cancel();

        // Start Initialization
        _initializeWorker = new Thread(() =>
        {
            // Kill lingering threads
            TeardownAllAndReset();

            // Load all modules
            AvailableModules = LoadAssembliesFromPath(GetModulePaths());

            // Attempt to initialize the requested runtimes.
            if (AvailableModules != null)
                InitRequestedRuntimes(AvailableModules);
            else _logger.LogWarning("No modules loaded.");

        });
        _logger.LogInformation("Starting initialization tracking");
        _initializeWorker.Start();
    }

    private static string[] GetModulePaths()
    {
        if (!Directory.Exists(Utils.CustomLibsDirectory))
            Directory.CreateDirectory(Utils.CustomLibsDirectory);

        return Directory.GetFiles(Utils.CustomLibsDirectory, "*.dll");
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
        catch (Exception e)
        {
            _logger.LogError(e.Message + ". Skipping...");
        }

        return null;
    }

    public static List<Assembly> LoadAssembliesFromPath(string[] path)
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

        return returnList;
    }

    private static void EnsureModuleThreadStarted(ExtTrackingModule module)
    {
        foreach (var pair in ModuleThreads)
            if (pair.module == module)
                return;

        var cts = new CancellationTokenSource();
        ThreadPool.QueueUserWorkItem(state =>
        {
            var token = (CancellationToken)state;
            while(!token.IsCancellationRequested)
            {
                module.Update();
            }
        }, cts.Token);

        var _pair = new ModuleThread();
        _pair.module = module;
        _pair.token = cts;

        ModuleThreads.Add(_pair);
    }

    private void AttemptModuleInitialize(ExtTrackingModule module)
    {
        if (!module.Supported.SupportsEye && !module.Supported.SupportsExpression)
            return;

        module.Logger = _loggerFactory.CreateLogger(module.GetType().Name);

        bool eyeSuccess = false, expressionSuccess = false;
        try
        {
            (eyeSuccess, expressionSuccess) = module.Initialize(EyeStatus == ModuleState.Uninitialized, ExpressionStatus == ModuleState.Uninitialized);
        }
        catch (MissingMethodException)
        {
            _logger.LogError(module.GetType().Name + " does not properly implement ExtTrackingModule. Skipping.");
            return;
        }
        catch (Exception e)
        {
            _logger.LogError("Exception initializing " + module.GetType().Name + ". Skipping.");
            _logger.LogError(e.Message);
            return;
        }

        module.ModuleInformation.OnActiveChange = state =>
            module.Status = state ? ModuleState.Active : ModuleState.Idle;

        // Skip any modules that don't succeed, otherwise set UnifiedLib to have these states active and add module to module list.
        if (!eyeSuccess && !expressionSuccess) return;
        EyeStatus = eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        EyeStatus = expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;

        module.ModuleInformation.Active = true;
        _dispatcherService.Run(() =>
        {
            if (!ModuleMetadatas.Contains(module.ModuleInformation))
                ModuleMetadatas.Add(module.ModuleInformation);
        });
        EnsureModuleThreadStarted(module);
    }

    private void InitRequestedRuntimes(List<Assembly> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach (Assembly module in moduleType)
        {
            _logger.LogInformation("Initializing module: " + module.ToString());
            var loadedModule = LoadExternalModule(module);
            AttemptModuleInitialize(loadedModule);
        }

        foreach (var pair in ModuleThreads)
        {
            if (pair.module.ModuleInformation.Active)
                _logger.LogInformation("Tracking initialized via " + pair.module.ToString());
        }
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public void TeardownAllAndReset()
    {
        foreach (var modulePair in ModuleThreads)
        {
            _logger.LogInformation("Teardown: " + modulePair.module.GetType().Name);
            modulePair.module.Teardown();
            modulePair.token.Cancel();
            _logger.LogInformation("Teardown complete: " + modulePair.module.GetType().Name);
        }

        ModuleThreads.Clear();
        ModuleThreads.Clear();
        
        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }
}