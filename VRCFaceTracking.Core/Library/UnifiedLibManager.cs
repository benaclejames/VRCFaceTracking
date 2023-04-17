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
    private static ILogger _logger;
    private static ILoggerFactory _loggerFactory;
    
    public ObservableCollection<ModuleMetadata> ModuleMetadatas { get; set; }
    private readonly IDispatcherService _dispatcherService;

    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger("UnifiedLibManager");
        _dispatcherService = dispatcherService;

        ModuleMetadatas = new ObservableCollection<ModuleMetadata>();
    }

    #region Statuses
    public static ModuleState EyeStatus { get; private set; }
    public static ModuleState ExpressionStatus { get; private set; }
    #endregion

    #region Modules
    public static List<Assembly> AvailableModules { get; private set; }
    internal static List<Assembly> RequestedModules = new();
    internal static List<ExtTrackingModule> ActiveModules = new();
    private static readonly Dictionary<ExtTrackingModule, CancellationTokenSource> UsefulThreads = new();
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

    public static void ReloadModules()
    {
        AvailableModules = LoadExternalAssemblies(GetAllModulePaths());
    }

    private static string[] GetAllModulePaths()
    {
        List<string> modulePaths = new List<string>();

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

    public static List<Assembly> LoadExternalAssemblies(string[] path)
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
        if (UsefulThreads.ContainsKey(module))
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
        UsefulThreads.Add(module, cts);
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

        ActiveModules.Add(module);
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
        /*_dispatcherService.Run(() => Modules.Add(new()
        {
            Active = false,
            Name = "Loading Modules..."
        }));*/

        _logger.LogInformation("Initializing runtimes...");

        foreach (Assembly module in moduleType)
        {
            _logger.LogInformation("Initializing module: " + module.ToString());
            var loadedModule = LoadExternalModule(module);
            AttemptModuleInitialize(loadedModule);
        }

        foreach (ExtTrackingModule module in ActiveModules)
        {
            if (module.ModuleInformation.Active)
                _logger.LogInformation("Tracking initialized using " + module.ToString());
        }
        
        // If both modules are uninitialized, we can't do anything
        /*if (EyeStatus == ModuleState.Uninitialized && ExpressionStatus == ModuleState.Uninitialized)
        {
            _dispatcherService.Run(() => Modules[0] = new ModuleMetadata()
                {
                    Name = "Couldn't load any modules!",
                    Active = false
                }
            );
        }
        else
        {
            // Remove the loading module
            _dispatcherService.Run(() => Modules.RemoveAt(0));
        }*/
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public void TeardownAllAndReset()
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

        ActiveModules.Clear();

        //_dispatcherService.Run(() => Modules.Clear());
    }
}