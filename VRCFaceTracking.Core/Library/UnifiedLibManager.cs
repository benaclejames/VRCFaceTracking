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
        public Thread thread;
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
            TeardownAllAndResetAsync();

            // Find all modules
            var modules = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules());
            var modulePaths = modules.Select(m => m.AssemblyLoadPath);

            // Load all modules
            AvailableModules = LoadAssembliesFromPath(modulePaths.ToArray());

            // Attempt to initialize the requested runtimes.
            if (AvailableModules != null)
            {
                _logger.LogDebug("Initializing requested runtimes...");
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
                
                var references = loaded.GetReferencedAssemblies();
                var oldRefs = false;
                foreach (var reference in references)
                {
                    if (reference.Name == "VRCFaceTracking" || reference.Name == "VRCFaceTracking.Core")
                    {
                        if (reference.Version < new Version(5, 0, 0, 0))
                        {
                            _logger.LogWarning("Module {dll} references an older version of VRCFaceTracking. Skipping.", Path.GetFileName(dll));
                            oldRefs = true;
                        }
                    }
                }
                if (oldRefs) continue;
                
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
            return;

        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            _logger.LogDebug("Starting thread for {module}", module.GetType().Name);
            while (!cts.IsCancellationRequested)
            {
                module.Update();
            }
            _logger.LogDebug("Thread for {module} ended", module.GetType().Name);
        });
        thread.Start();

        var _pair = new ModuleThread
        {
            module = module,
            token = cts,
            alc = AssemblyLoadContext.GetLoadContext(module.GetType().Assembly),
            thread = thread
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

        bool eyeSuccess, expressionSuccess;
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
        if (!eyeSuccess && !expressionSuccess)
            return;
        
        EyeStatus = eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        ExpressionStatus = expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        
        module.ModuleInformation.Active = true;
        module.ModuleInformation.UsingEye = eyeSuccess;
        module.ModuleInformation.UsingExpression = expressionSuccess;
        _dispatcherService.Run(() => { if (!ModuleMetadatas.Contains(module.ModuleInformation)) ModuleMetadatas.Add(module.ModuleInformation); });
        if (module.UseUpdateThread)
            EnsureModuleThreadStarted(module);
    }

    private void InitRequestedRuntimes(List<Assembly> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach (Assembly module in moduleType)
        {
            if (EyeStatus > ModuleState.Uninitialized && ExpressionStatus > ModuleState.Uninitialized)
            {
                break;
            }
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
    public void TeardownAllAndResetAsync()
    {
        _logger.LogInformation("Tearing down all modules...");

        foreach (var modulePair in ModuleThreads)
        {
            _logger.LogInformation("Tearing down {module} ", modulePair.module.GetType().Name);
            modulePair.token.Cancel();
            if (modulePair.thread.IsAlive)
            {
                // Edge case, we wait for the thread to finish before unloading the assembly
                _logger.LogDebug("Waiting for {module}'s thread to join...", modulePair.module.GetType().Name);
                modulePair.thread.Join();
            }

            modulePair.module.Teardown();
            ModuleMetadatas.Remove(modulePair.module.ModuleInformation);
        }

        ModuleThreads.Clear();
        
        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }
}