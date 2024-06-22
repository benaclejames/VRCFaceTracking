using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Sentry.Protocol;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Library;

public class UnifiedLibManager : ILibManager
{
    #region Logger
    private readonly ILogger<UnifiedLibManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    #endregion

    #region Observables
    public ObservableCollection<ModuleMetadata> LoadedModulesMetadata { get; set; }
    private readonly IDispatcherService _dispatcherService;
    #endregion

    #region Statuses
    public static ModuleState EyeStatus { get; private set; }
    public static ModuleState ExpressionStatus { get; private set; }
    #endregion

    #region Modules

    private List<Assembly> AvailableModules { get; set; }
    private readonly List<ModuleRuntimeInfo> _moduleThreads = new();
    private readonly IModuleDataService _moduleDataService;

    private string _sandboxProcessPath { get; set; }
    private List<ModuleRuntimeInfo> AvailableSandboxModules = new ();
    #endregion

    #region Thread
    private Thread _initializeWorker;
    private VrcftSandboxServer _sandboxServer;
    #endregion

    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService, IModuleDataService moduleDataService)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger<UnifiedLibManager>();
        _dispatcherService = dispatcherService;
        _moduleDataService = moduleDataService;

        LoadedModulesMetadata = new ObservableCollection<ModuleMetadata>();
        _sandboxProcessPath = Path.GetFullPath("VRCFaceTracking.ModuleProcess.exe");
        if ( !File.Exists(_sandboxProcessPath) )
        {
            // @TODO: Better error handling
            throw new FileNotFoundException($"Failed to find sandbox process at \"{_sandboxProcessPath}\"!");
        }
    }

    public void Initialize()
    {
        LoadedModulesMetadata.Clear();
        LoadedModulesMetadata.Add(new ModuleMetadata
        { 
            Active = false,
            Name = "Initializing Modules..."
        });

        // Spawn sandbox server if it's null
        if (_sandboxServer == null )
        {
            // @TODO: Figure out an elegant way to ask the GUI for the ports the user assigned to the OSCTarget.
            int[] reservedPorts = new int[2] { 9000, 9001 };
            _sandboxServer = new VrcftSandboxServer(_loggerFactory, reservedPorts);
            _sandboxServer.OnPacketReceived += (in IpcPacket packet, in int port) =>
            {
                // Get sandbox module internal index
                int moduleIndex = -1;
                for ( int i = 0; i < AvailableSandboxModules.Count; i++ )
                {
                    if ( AvailableSandboxModules[i].SandboxProcessPort == port )
                    {
                        moduleIndex = i;
                        break;
                    }
                }

                switch ( packet.GetPacketType() )
                {
                    // @TODO: Move these all into methods to make the code easier to maintain
                    case IpcPacket.PacketType.Handshake:
                        {
                            // Look for the PID in the added modules list
                            var pkt = (HandshakePacket) packet;
                            lock ( AvailableSandboxModules )
                            {
                                for ( int i = 0; i < AvailableSandboxModules.Count; i++ )
                                {
                                    if ( AvailableSandboxModules[i].SandboxProcessPID == pkt.PID )
                                    {
                                        var structCopy = AvailableSandboxModules[i];
                                        structCopy.SandboxProcessPort = port;
                                        AvailableSandboxModules[i] = structCopy;

                                        _logger.LogInformation("Initializing {module}...", AvailableSandboxModules[i].ModuleClassName.ToString());
                                        AttemptSandboxedModuleInitialize(AvailableSandboxModules[i]);

                                        break;
                                    }
                                }
                            }
                            break;
                        }

                    case IpcPacket.PacketType.EventLog:
                        {
                            EventLogPacket eventLogPacket = (EventLogPacket) packet;
                            _logger.Log(eventLogPacket.LogLevel, eventLogPacket.Message);
                            break;
                        }

                    case IpcPacket.PacketType.ReplyGetSupported:
                        {
                            // We now know whether or not the module supports face or eye tracking
                            ReplySupportedPacket replySupportedPacket = (ReplySupportedPacket) packet;
                            // Now tell it to initialise
                            EventInitPacket eventInitPacket = new EventInitPacket()
                            {
                                expressionAvailable     = ExpressionStatus == ModuleState.Uninitialized,
                                eyeAvailable            = EyeStatus == ModuleState.Uninitialized,
                            };
                            _sandboxServer.SendData(eventInitPacket, port);
                            break;
                        }

                    case IpcPacket.PacketType.ReplyInit:
                        {
                            ReplyInitPacket replyInitPacket = (ReplyInitPacket) packet;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.Name = replyInitPacket.ModuleInformationName;

                            AvailableSandboxModules[moduleIndex].ModuleInformation.OnActiveChange = state =>
                                AvailableSandboxModules[moduleIndex].Status = state ? ModuleState.Active : ModuleState.Idle;

                            // Skip any modules that don't succeed, otherwise set UnifiedLib to have these states active and add module to module list.
                            if ( !replyInitPacket.eyeSuccess && !replyInitPacket.expressionSuccess )
                            {
                                break;
                            }

                            EyeStatus           = replyInitPacket.eyeSuccess        ? ModuleState.Active : ModuleState.Uninitialized;
                            ExpressionStatus    = replyInitPacket.expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;

                            AvailableSandboxModules[moduleIndex].ModuleInformation.Active           = true;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.UsingEye         = replyInitPacket.eyeSuccess;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.UsingExpression  = replyInitPacket.expressionSuccess;
                            
                            _dispatcherService.Run(() => {
                                if ( !LoadedModulesMetadata.Contains(AvailableSandboxModules[moduleIndex].ModuleInformation) )
                                {
                                    LoadedModulesMetadata.Add(AvailableSandboxModules[moduleIndex].ModuleInformation);
                                }



                                if ( LoadedModulesMetadata.Count == 0 )
                                {
                                    _logger.LogWarning("No modules loaded.");
                                    _dispatcherService.Run(() =>
                                    {
                                        LoadedModulesMetadata.Clear();
                                        LoadedModulesMetadata.Add(new ModuleMetadata
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
                                        LoadedModulesMetadata.RemoveAt(0);
                                    });
                                    foreach ( var pair in _moduleThreads )
                                    {
                                        if ( pair.ModuleInformation.Active )
                                        {
                                            _logger.LogInformation("Tracking initialized via {module}", pair.ModuleClassName.ToString());
                                        }
                                    }
                                }
                            });

                            /*
                             
                            @TODO: Move to update thread
                             */
                            break;
                        }
                }
            };
        }

        // Start Initialization
        _initializeWorker = new Thread(() =>
        {
            // Kill lingering threads
            TeardownAllAndResetAsync();

            // Find all modules
            var modules = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules());
            var modulePaths = modules.Select(m => m.AssemblyLoadPath);

#if false
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
                    LoadedModulesMetadata.Clear();
                    LoadedModulesMetadata.Add(new ModuleMetadata
                    {
                        Active = false,
                        Name = "No Modules Loaded"
                    });
                });
                _logger.LogWarning("No modules loaded.");
            }
#endif
            // Load all modules
            AvailableSandboxModules.Clear();
            InitialiseSandboxesBaseOnPaths(modulePaths.ToArray());

            if ( AvailableSandboxModules != null && AvailableSandboxModules.Count > 0 )
            {
                _logger.LogDebug("Initializing requested runtimes...");
            }
            else
            {
                _dispatcherService.Run(() =>
                {
                    LoadedModulesMetadata.Clear();
                    LoadedModulesMetadata.Add(new ModuleMetadata
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

    private ExtTrackingModule LoadExternalModule(Assembly dll)
    {
        _logger.LogInformation("Loading External Module " + dll.FullName);

        try
        {
            // Get the first class that implements ExtTrackingModule
            var module = dll.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(ExtTrackingModule)));
            if (module == null)
            {
                throw new Exception("Failed to get module's ExtTrackingModule impl");
            }
            var moduleObj = (ExtTrackingModule)Activator.CreateInstance(module);

            return moduleObj;
        }
        catch (Exception e)
        {
            _logger.LogError("Exception loading {dll}. Skipping. {e}", dll.FullName, e);
        }

        return null;
    }

    private void InitialiseSandboxesBaseOnPaths(IEnumerable<string> paths)
    {
        foreach ( var dll in paths )
        {
            try
            {
                // Start subprocess
                var sandboxProcess  = Process.Start(new ProcessStartInfo(_sandboxProcessPath, $"--port {_sandboxServer.Port} --module-path \"{dll}\""));
                var pid             = sandboxProcess.Id;

                // Add the module info into the loaded list
                ModuleRuntimeInfo runtimeInfo = new ModuleRuntimeInfo()
                {
                    SandboxProcessPID   = pid,
                    SandboxProcessPort  = -1,
                    SandboxModulePath   = dll,
                    IsActive            = true,
                    Process             = sandboxProcess,
                    ModuleClassName     = Path.GetFileNameWithoutExtension(dll),
                    ModuleInformation   = new (),
                    EventBus            = new ()
                };
                lock ( AvailableSandboxModules )
                {
                    AvailableSandboxModules.Add(runtimeInfo);
                }
            }
            catch ( Exception e )
            {
                _logger.LogWarning("{error} Failed to start sandbox process for {path}. Skipping...", e.Message, dll);
            }
        }
    }

    private List<Assembly> LoadAssembliesFromPath(IEnumerable<string> path)
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
                if (oldRefs)
                {
                    continue;
                }

                foreach(var type in loaded.GetExportedTypes())
                {
                    if (type.BaseType != typeof(ExtTrackingModule))
                    {
                        continue;
                    }

                    _logger.LogDebug("{module} properly implements ExtTrackingModule.", type.Name);
                    returnList.Add(loaded);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("{error} Assembly not able to be loaded. Skipping.", e.Message);
            }
        }

        return returnList;
    }

    private void EnsureModuleThreadStarted(ExtTrackingModule module)
    {
        if (_moduleThreads.Any(pair => pair.Module == module))
        {
            return;
        }

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

        var runtimeModules = new ModuleRuntimeInfo
        {
            Module = module,
            UpdateCancellationToken = cts,
            AssemblyLoadContext = AssemblyLoadContext.GetLoadContext(module.GetType().Assembly),
            UpdateThread = thread
        };

        _moduleThreads.Add(runtimeModules);
    }

    private void AttemptModuleInitialize(ExtTrackingModule module)
    {
        if (module.Supported is { SupportsEye: false, SupportsExpression: false })
        {
            return;
        }

        module.Logger = _loggerFactory.CreateLogger(module.GetType().Name);

        var shouldEyeInitialize = EyeStatus == ModuleState.Uninitialized;
        var shouldExpressionInitialize = ExpressionStatus == ModuleState.Uninitialized;
        bool eyeSuccess, expressionSuccess;
        try
        {
            (eyeSuccess, expressionSuccess) = module.Initialize(shouldEyeInitialize, shouldExpressionInitialize);
        }
        catch (MissingMethodException)
        {
            _logger.LogError("{moduleName} does not properly implement ExtTrackingModule. Skipping.", module.GetType().Name);
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
        {
            return;
        }
        
        if (shouldEyeInitialize)
        {
            EyeStatus = eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        }

        if (shouldExpressionInitialize)
        {
            ExpressionStatus = expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        }

        module.ModuleInformation.Active = true;
        module.ModuleInformation.UsingEye = shouldEyeInitialize && eyeSuccess;
        module.ModuleInformation.UsingExpression = shouldExpressionInitialize && expressionSuccess;
        _dispatcherService.Run(() => { 
            if (!LoadedModulesMetadata.Contains(module.ModuleInformation))
            {
                LoadedModulesMetadata.Add(module.ModuleInformation);
            }
        });
        EnsureModuleThreadStarted(module);
    }
    private void AttemptSandboxedModuleInitialize(ModuleRuntimeInfo module)
    {
        // Tell the sandbox to call the initialize function on the module
        var eventGetSupportedPacket = new EventInitGetSupported();

        // If PID is valid and we know which port the sandbox process is running on
        if ( module.SandboxProcessPID != -1 && module.SandboxProcessPort > 0 )
        {
            _sandboxServer.SendData(eventGetSupportedPacket, module.SandboxProcessPort);
        }
        else
        {
            // Queue the packet so that we send it after we know which process the sandbox process is running on
            QueuedPacket queuedPacket = new QueuedPacket()
            {
                packet = eventGetSupportedPacket,
                destinationPort = module.SandboxProcessPort
            };
            module.EventBus.Enqueue(queuedPacket);
        }
    }

    private void InitRequestedRuntimes(List<Assembly> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach (var module in moduleType.TakeWhile(_ => EyeStatus <= ModuleState.Uninitialized || ExpressionStatus <= ModuleState.Uninitialized))
        {
            _logger.LogInformation("Initializing {module}", module.ToString());
            var loadedModule = LoadExternalModule(module);
            AttemptModuleInitialize(loadedModule);
        }

        if (_moduleThreads.Count == 0)
        {
            _logger.LogWarning("No modules loaded.");
            _dispatcherService.Run(() =>
            {
                LoadedModulesMetadata.Clear();
                LoadedModulesMetadata.Add(new ModuleMetadata
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
                LoadedModulesMetadata.RemoveAt(0);
            });
            foreach (var pair in _moduleThreads)
            {
                if (pair.Module.ModuleInformation.Active)
                {
                    _logger.LogInformation("Tracking initialized via {module}", pair.Module.ToString());
                }
            }
        }
    }

    private void InitRequestedRuntimesSandboxed(List<ModuleRuntimeInfo> moduleType)
    {
        _logger.LogInformation("Initializing runtimes...");

        foreach ( var module in moduleType.TakeWhile(_ => EyeStatus <= ModuleState.Uninitialized || ExpressionStatus <= ModuleState.Uninitialized) )
        {
            _logger.LogInformation("Initializing {module}", module.ToString());
            // var loadedModule = LoadExternalModule(module);
            AttemptSandboxedModuleInitialize(module);
        }

        if ( _moduleThreads.Count == 0 )
        {
            _logger.LogWarning("No modules loaded.");
            _dispatcherService.Run(() =>
            {
                LoadedModulesMetadata.Clear();
                LoadedModulesMetadata.Add(new ModuleMetadata
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
                LoadedModulesMetadata.RemoveAt(0);
            });
            foreach ( var pair in _moduleThreads )
            {
                if ( pair.Module.ModuleInformation.Active )
                {
                    _logger.LogInformation("Tracking initialized via {module}", pair.Module.ToString());
                }
            }
        }
    }

    private bool TeardownModule(ModuleRuntimeInfo module)
    {
        _logger.LogInformation("Tearing down {module} ", module.Module.GetType().Name);
        module.UpdateCancellationToken.Cancel();
        if (module.UpdateThread.IsAlive)
        {
            // Edge case, we wait for the thread to finish before unloading the assembly
            _logger.LogDebug("Waiting for {module}'s thread to join...", module.Module.GetType().Name);
            // Specify a timeout in milliseconds
            var timeoutMilliseconds = 1000;
            if (!module.UpdateThread.Join(timeoutMilliseconds))
            {
                _logger.LogWarning("{module}'s thread did not finish within the timeout period.", module.Module.GetType().Name);
            }
        }

        module.Module.Teardown();
        module.AssemblyLoadContext.Unload();
        
        return true;
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public void TeardownAllAndResetAsync()
    {
        _logger.LogInformation("Tearing down all modules...");

        foreach ( var module in _moduleThreads )
        {
            var success = false;
            try
            {
                success = TeardownModule(module);
            } finally
            {
                if ( !success )
                {
                    _logger.LogWarning($"Module: {module.Module.ModuleInformation.Name} failed to shut down. Killing its thread.");
                    module.UpdateThread.Interrupt();
                }
            }
        }

        _moduleThreads.Clear();

        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }

    // Signal all active modules to gracefully shut down their respective runtimes
    public void TeardownAllAndResetAsyncLegacy()
    {
        _logger.LogInformation("Tearing down all modules...");

        foreach ( var module in _moduleThreads )
        {
            var success = false;
            try
            {
                success = TeardownModule(module);
            } finally
            {
                if ( !success )
                {
                    _logger.LogWarning($"Module: {module.Module.ModuleInformation.Name} failed to shut down. Killing its thread.");
                    module.UpdateThread.Interrupt();
                }
            }
        }

        _moduleThreads.Clear();

        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }
}