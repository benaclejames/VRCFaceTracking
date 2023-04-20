using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
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
    public Action<string[]> OnLoad { get; set; }
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
        public AssemblyLoadContext alc;
    }
    public static List<Assembly> AvailableModules { get; private set; }
    private static readonly List<ModuleThread> ModuleThreads = new();
    private static IModuleDataService _moduleDataService;
    #endregion

    #region Thread
    private static Thread _initializeWorker;
    private static readonly CancellationTokenSource _initCts;
    #endregion

    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService, IModuleDataService moduleDataService)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger("UnifiedLibManager");
        _dispatcherService = dispatcherService;
        _moduleDataService = moduleDataService;

        ModuleMetadatas = new ObservableCollection<ModuleMetadata>();
    }

    public void Initialize()
    {
        if (_initializeWorker != null && _initializeWorker.IsAlive && _initCts != null) _initCts.Cancel();
        
        
            ModuleMetadatas.Clear();
            ModuleMetadatas.Add(new ModuleMetadata
            {
                Active = false,
                Name = "Initializing Modules..."
            });

        // Start Initialization
        _initializeWorker = new Thread(() =>
        {
            // Kill lingering threads
            TeardownAllAndReset();

            // Find all modules
            var modulePaths = _moduleDataService.GetLegacyModules();
            foreach (var module in _moduleDataService.GetInstalledModulesAsync().Result)
            {
                if (!string.IsNullOrEmpty(module.AssemblyLoadPath))
                {
                    modulePaths = modulePaths.Append(module.AssemblyLoadPath);
                }
            }
            
            // Load all modules
            AvailableModules = LoadAssembliesFromPath(modulePaths.ToArray());

            // Attempt to initialize the requested runtimes.
            if (AvailableModules != null)
            {
                OnLoad?.Invoke(modulePaths.ToArray());
                InitRequestedRuntimes(AvailableModules);
            }
            else
            {
                _dispatcherService.Run(() =>
                {
                    ModuleMetadatas.Clear();
                    ModuleMetadatas.Add(new ModuleMetadata
                    {
                        Active = false,
                        Name = "No Modules Loaded"
                    });
                });
                _logger.LogWarning("No modules loaded.");
            }

        });
        _logger.LogInformation("Starting initialization tracking");
        _initializeWorker.Start();
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
        catch (Exception e)
        {
            _logger.LogError("Exception loading {dll}. Skipping. {e}", dll.FullName, e);
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
                var alc = new AssemblyLoadContext(dll, true);
                var loaded = alc.LoadFromAssemblyPath(dll);
                foreach(var type in loaded.GetExportedTypes())
                {
                    if (type.BaseType == typeof(ExtTrackingModule))
                    {
                        _logger.LogDebug("{module} properly implements ExtTrackingModule.", type.Name);
                        returnList.Add(loaded);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e.Message + " Assembly not able to be loaded. Skipping.");
            }
        }

        return returnList;
    }

    private static void EnsureModuleThreadStarted(ExtTrackingModule module)
    {
        if (ModuleThreads.Any(pair => pair.module == module))
        {
            AssemblyLoadContext.GetLoadContext(module.GetType().Assembly).Unload();
            return;
        }

        var cts = new CancellationTokenSource();
        ThreadPool.QueueUserWorkItem(state =>
        {
            var token = (CancellationToken)state;
            while(!token.IsCancellationRequested)
            {
                module.Update();
            }
        }, cts.Token);

        var _pair = new ModuleThread
        {
            module = module,
            token = cts,
            alc = AssemblyLoadContext.GetLoadContext(module.GetType().Assembly)
        };

        ModuleThreads.Add(_pair);
    }

    private void AttemptModuleInitialize(ExtTrackingModule module)
    {
        if (!module.Supported.SupportsEye && !module.Supported.SupportsExpression)
        {
            return;
        }

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
            _logger.LogError("Exception initializing {module}. Skipping. {e}", module.GetType().Name, e);
            return;
        }

        module.ModuleInformation.OnActiveChange = state =>
            module.Status = state ? ModuleState.Active : ModuleState.Idle;

        // Skip any modules that don't succeed, otherwise set UnifiedLib to have these states active and add module to module list.
        if (!eyeSuccess && !expressionSuccess) return;
        EyeStatus = eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        EyeStatus = expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        
        module.ModuleInformation.Active = true;
        module.ModuleInformation.UsingEye = eyeSuccess;
        module.ModuleInformation.UsingExpression = expressionSuccess;
        _dispatcherService.Run(() =>
        {
            if (!ModuleMetadatas.Contains(module.ModuleInformation))
            {
                ModuleMetadatas.Add(module.ModuleInformation);
            }
        });
        EnsureModuleThreadStarted(module);
    }

    private void InitRequestedRuntimes(List<Assembly> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach (Assembly module in moduleType)
        {
            _logger.LogInformation("Initializing {module}", module.ToString());
            var loadedModule = LoadExternalModule(module);
            AttemptModuleInitialize(loadedModule);
        }

        if (ModuleThreads.Count == 0)
        {
            _logger.LogWarning("No modules loaded.");
            _dispatcherService.Run(() =>
            {
                ModuleMetadatas.Clear();
                ModuleMetadatas.Add(new ModuleMetadata
                {
                    Active = false,
                    Name = "No Modules Loaded"
                });
            });
        }
        else
        {
            _dispatcherService.Run(() =>
            {
                // Remove our dummy module
                ModuleMetadatas.RemoveAt(0);
            });
            foreach (var pair in ModuleThreads)
            {
                if (pair.module.ModuleInformation.Active)
                {
                    _logger.LogInformation("Tracking initialized via {module}", pair.module.ToString());
                }
            }
        }
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public void TeardownAllAndReset()
    {
        _logger.LogInformation("Tearing down all modules...");
        foreach (var modulePair in ModuleThreads)
        {
            _logger.LogInformation("Tearing down {module} ", modulePair.module.GetType().Name);
            modulePair.module.Teardown();
            modulePair.token.Cancel();
        }

        ModuleThreads.Clear();
        ModuleThreads.Clear();
        
        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }
}