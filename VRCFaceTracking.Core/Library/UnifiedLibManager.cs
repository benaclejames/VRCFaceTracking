using System;
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
    private readonly ILogger _moduleLogger;
    private readonly ILoggerFactory _loggerFactory;
    #endregion

    #region Observables
    public ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; }
    private bool _hasInitializedAtLeastOneModule = false;
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
    private static VrcftSandboxServer _sandboxServer;
    #endregion
    
    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService, IModuleDataService moduleDataService)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger<UnifiedLibManager>();
        _moduleLogger = factory.CreateLogger("\0VRCFT\0");
        _dispatcherService = dispatcherService;
        _moduleDataService = moduleDataService;

        LoadedModulesMetadata = new ObservableCollection<ModuleMetadataInternal>();
        _sandboxProcessPath = Path.GetFullPath("VRCFaceTracking.ModuleProcess.exe");
        if ( !File.Exists(_sandboxProcessPath) )
        {
            // @TODO: Better error handling
            throw new FileNotFoundException($"Failed to find sandbox process at \"{_sandboxProcessPath}\"!");
        }

        // @TODO: Kill any lingering sub-modules to eliminate any conflicts
    }

    public void Initialize()
    {
        LoadedModulesMetadata.Clear();
        LoadedModulesMetadata.Add(new ModuleMetadataInternal
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
                                bool pidRegistered = false;
                                
                                for ( int i = 0; i < AvailableSandboxModules.Count; i++ )
                                {
                                    if ( AvailableSandboxModules[i].SandboxProcessPID == pkt.PID )
                                    {
                                        var structCopy = AvailableSandboxModules[i];
                                        structCopy.SandboxProcessPort = port;
                                        AvailableSandboxModules[i] = structCopy;

                                        _logger.LogInformation("Initializing {module}...", AvailableSandboxModules[i].ModuleClassName.ToString());
                                        AttemptSandboxedModuleInitialize(AvailableSandboxModules[i]);
                                        pidRegistered = true;

                                        break;
                                    }
                                }

                                if ( pidRegistered == false )
                                {
                                    Process sandboxProcess = Process.GetProcessById(pkt.PID);

                                    ModuleRuntimeInfo runtimeInfo = new ModuleRuntimeInfo()
                                    {
                                        SandboxProcessPID   = pkt.PID,
                                        SandboxProcessPort  = port,
                                        SandboxModulePath   = pkt.ModulePath,
                                        IsActive            = true,
                                        Process             = sandboxProcess,
                                        ModuleClassName     = Path.GetFileNameWithoutExtension(pkt.ModulePath),
                                        ModuleInformation   = new (),
                                        EventBus            = new (),
                                    };
                                    AvailableSandboxModules.Add(runtimeInfo);

                                    _logger.LogInformation("Initializing {module}...", runtimeInfo.ModuleClassName.ToString());
                                    AttemptSandboxedModuleInitialize(runtimeInfo);
                                    pidRegistered = true;
                                }
                            }
                            break;
                        }

                    case IpcPacket.PacketType.EventLog:
                        {
                            EventLogPacket eventLogPacket = (EventLogPacket) packet;
                            _moduleLogger.Log(eventLogPacket.LogLevel, eventLogPacket.Message);
                            break;
                        }

                    case IpcPacket.PacketType.ReplyGetSupported:
                        {
                            // We now know whether or not the module supports face or eye tracking
                            ReplySupportedPacket replySupportedPacket = (ReplySupportedPacket) packet;

                            AvailableSandboxModules[moduleIndex].SupportsEyeTracking        = AvailableSandboxModules[moduleIndex].SupportsEyeTracking && replySupportedPacket.eyeAvailable;
                            AvailableSandboxModules[moduleIndex].SupportsExpressionTracking = AvailableSandboxModules[moduleIndex].SupportsExpressionTracking && replySupportedPacket.expressionAvailable;

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

                            // Update support variables
                            AvailableSandboxModules[moduleIndex].SupportsEyeTracking        = AvailableSandboxModules[moduleIndex].SupportsEyeTracking && replyInitPacket.eyeSuccess;
                            AvailableSandboxModules[moduleIndex].SupportsExpressionTracking = AvailableSandboxModules[moduleIndex].SupportsExpressionTracking && replyInitPacket.expressionSuccess;

                            // Skip any modules that don't succeed, otherwise set UnifiedLib to have these states active and add module to module list.
                            if ( !replyInitPacket.eyeSuccess && !replyInitPacket.expressionSuccess )
                            {
                                break;
                            }

                            int portCopy = port; // So that we can use it in the lambda method
                            AvailableSandboxModules[moduleIndex].ModuleInformation.OnActiveChange = (state) =>
                            {
                                AvailableSandboxModules[moduleIndex].Status = state ? ModuleState.Active : ModuleState.Idle;

                                EventStatusUpdatePacket statusUpdatePkt = new EventStatusUpdatePacket();
                                statusUpdatePkt.ModuleState = AvailableSandboxModules[moduleIndex].Status;
                                _sandboxServer.SendData(statusUpdatePkt, portCopy);
                            };

                            EyeStatus = replyInitPacket.eyeSuccess        ? ModuleState.Active : ModuleState.Uninitialized;
                            ExpressionStatus    = replyInitPacket.expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;

                            AvailableSandboxModules[moduleIndex].ModuleInformation.Active           = true;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.UsingEye         = replyInitPacket.eyeSuccess;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.UsingExpression  = replyInitPacket.expressionSuccess;
                            AvailableSandboxModules[moduleIndex].ModuleInformation.StaticImages     = replyInitPacket.IconDataStreams;
                            EnsureModuleThreadStartedSandboxed(AvailableSandboxModules[moduleIndex]);

                            _dispatcherService.Run(() => {
                                if ( !LoadedModulesMetadata.Contains(AvailableSandboxModules[moduleIndex].ModuleInformation) )
                                {
                                    LoadedModulesMetadata.Add(AvailableSandboxModules[moduleIndex].ModuleInformation);
                                }

                                if ( AvailableSandboxModules.Count == 0 )
                                {
                                    _logger.LogWarning("No modules loaded.");
                                    LoadedModulesMetadata.Clear();
                                    LoadedModulesMetadata.Add(new ModuleMetadataInternal
                                    {
                                        Active = false,
                                        Name = "No Modules Loaded"
                                    });
                                }
                                else
                                {
                                    // Remove our dummy module
                                    if ( LoadedModulesMetadata.Count > 0 &&
                                        LoadedModulesMetadata[0].Active == false &&
                                           (LoadedModulesMetadata[0].Name == "No Modules Loaded" ||
                                            LoadedModulesMetadata[0].Name == "Initializing Modules..." ))
                                    {
                                        LoadedModulesMetadata.RemoveAt(0);
                                    }

                                    foreach ( var pair in _moduleThreads )
                                    {
                                        if ( pair.ModuleInformation.Active )
                                        {
                                            _logger.LogInformation("Tracking initialized via {module}", pair.ModuleClassName.ToString());
                                        }
                                    }
                                }
                            });

                            break;
                        }
                    case IpcPacket.PacketType.ReplyUpdate:
                        {
                            ReplyUpdatePacket replyUpdatePacket = (ReplyUpdatePacket) packet;

                            if ( AvailableSandboxModules[moduleIndex].Status == ModuleState.Active && AvailableSandboxModules[moduleIndex].ModuleInformation.Active )
                            {

                                if ( AvailableSandboxModules[moduleIndex].ModuleInformation.UsingEye )
                                {
                                    replyUpdatePacket.UpdateGlobalEyeState();
                                }
                                if ( AvailableSandboxModules[moduleIndex].ModuleInformation.UsingExpression )
                                {
                                    replyUpdatePacket.UpdateGlobalExpressionState();
                                }
                            }

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
                    LoadedModulesMetadata.Add(new ModuleMetadataInternal
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
                var sandboxProcess  = Process.Start(new ProcessStartInfo(
                    _sandboxProcessPath, $"--port {_sandboxServer.Port} --module-path \"{dll}\""
                ));

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
    
    private void EnsureModuleThreadStartedSandboxed(ModuleRuntimeInfo module)
    {
        if (_moduleThreads.Any(pair =>
            ( pair.SandboxProcessPID    == module.SandboxProcessPID ) &&
            ( pair.SandboxProcessPort   == module.SandboxProcessPort )
        ))
        {
            return;
        }

        int port = module.SandboxProcessPort;

        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            _logger.LogDebug("Starting thread for {module}", module.GetType().Name);
            var updatePacket = new EventUpdatePacket();
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(10); // Wait 10ms => 100Hz
                _sandboxServer.SendData(updatePacket, port);
            }
            _logger.LogDebug("Thread for {module} ended", module.GetType().Name);
        });
        thread.Start();
        module.UpdateCancellationToken = cts;
        module.UpdateThread = thread;

        _moduleThreads.Add(module);
    }

#if NO_SANDBOXING
    private void AttemptModuleInitialize(ExtTrackingModule module)
    {
        if (module.Supported is { SupportsEye: false, SupportsExpression: false })
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

        EyeStatus = eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        ExpressionStatus = expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        
        module.ModuleInformation.Active = true;
        module.ModuleInformation.UsingEye = eyeSuccess;
        module.ModuleInformation.UsingExpression = expressionSuccess;
        _dispatcherService.Run(() => {
            var moduleInternalForm = new ModuleMetadataInternal(module.ModuleInformation);
            if (!LoadedModulesMetadata.Contains(moduleInternalForm) )
            {
                LoadedModulesMetadata.Add(moduleInternalForm);
            }
        });
        EnsureModuleThreadStarted(module);
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
                LoadedModulesMetadata.Add(new ModuleMetadataInternal
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
#endif

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
                LoadedModulesMetadata.Add(new ModuleMetadataInternal
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
        if ( module.UpdateThread.IsAlive )
        {
            // Edge case, we wait for the thread to finish before unloading the assembly
            _logger.LogDebug("Waiting for {module}'s thread to join...", module.Module.GetType().Name);
            module.UpdateThread.Join();
        }

        module.Module.Teardown();
        module.AssemblyLoadContext.Unload();

        return true;
    }

    private bool TeardownModuleSandboxed(ModuleRuntimeInfo module)
    {
        _logger.LogInformation("Tearing down {module} ", module.ModuleClassName);

        // Send a message to the module sub-process
        var eventTeardownPacket = new EventTeardownPacket();
        _sandboxServer.SendData(eventTeardownPacket, module.SandboxProcessPort);

        // Kill the update thread
        module.UpdateCancellationToken.Cancel();
        if ( module.UpdateThread.IsAlive )
        {
            // Edge case, we wait for the thread to finish before unloading the assembly
            _logger.LogDebug("Waiting for {module}'s thread to join...", module.ModuleClassName);
            module.UpdateThread.Join();
        }

        // Give the module 100ms to kill itself
        Thread.Sleep(100);

        // @Note: Forcefully kill the process
        module.Process.Kill();

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
                success = TeardownModuleSandboxed(module);
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
                success = TeardownModuleSandboxed(module);
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