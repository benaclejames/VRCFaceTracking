using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Library;

public partial class UnifiedLibManager
{
    private void OnSandboxPacketReceived(in IpcPacket packet, in int port)
    {
        var moduleIndex = FindModuleIndexByPort(port);

        switch (packet.GetPacketType())
        {
            case IpcPacket.PacketType.Handshake:
                HandleHandshake((HandshakePacket)packet, port);
                break;

            case IpcPacket.PacketType.EventLog:
                _moduleLogger.Log(((EventLogPacket)packet).LogLevel, ((EventLogPacket)packet).Message);
                break;

            case IpcPacket.PacketType.ReplyGetSupported:
                HandleReplyGetSupported((ReplySupportedPacket)packet, port, moduleIndex);
                break;

            case IpcPacket.PacketType.ReplyInit:
                HandleReplyInit((ReplyInitPacket)packet, port, moduleIndex);
                break;

            case IpcPacket.PacketType.ReplyUpdate:
                HandleReplyUpdate((ReplyUpdatePacket)packet, moduleIndex);
                break;
        }
    }

    private int FindModuleIndexByPort(int port)
    {
        for (var i = 0; i < AvailableSandboxModules.Count; i++)
        {
            if (AvailableSandboxModules[i].SandboxProcessPort == port)
            {
                return i;
            }
        }
        return -1;
    }

    private void HandleHandshake(HandshakePacket pkt, int port)
    {
        lock (AvailableSandboxModules)
        {
            // Look for a module we already spawned with this PID.
            for (var i = 0; i < AvailableSandboxModules.Count; i++)
            {
                if (AvailableSandboxModules[i].SandboxProcessPID != pkt.PID)
                {
                    continue;
                }

                var existing = AvailableSandboxModules[i];
                existing.SandboxProcessPort = port;
                AvailableSandboxModules[i] = existing;

                _logger.LogInformation("Initializing {module}...", existing.ModuleClassName);
                AttemptSandboxedModuleInitialize(existing);
                return;
            }

            var runtimeInfo = new ModuleRuntimeInfo
            {
                SandboxProcessPID  = pkt.PID,
                SandboxProcessPort = port,
                SandboxModulePath  = pkt.ModulePath,
                IsActive           = true,
                Process            = Process.GetProcessById(pkt.PID),
                ModuleClassName    = Path.GetFileNameWithoutExtension(pkt.ModulePath),
                ModuleInformation  = new(),
                EventBus           = new(),
            };
            AvailableSandboxModules.Add(runtimeInfo);

            _logger.LogInformation("Initializing {module}...", runtimeInfo.ModuleClassName);
            AttemptSandboxedModuleInitialize(runtimeInfo);
        }
    }

    private async void HandleReplyGetSupported(ReplySupportedPacket reply, int port, int moduleIndex)
    {
        var module = AvailableSandboxModules[moduleIndex];
        var moduleConfig = await _moduleConfigurationService.GetInitializationConfig(module.ModuleMetadata.ModuleId);
        
        module.SupportsEyeTracking        = module.SupportsEyeTracking        && reply.eyeAvailable;
        module.SupportsExpressionTracking = module.SupportsExpressionTracking && reply.expressionAvailable;

        var initPacket = new EventInitPacket
        {
            expressionAvailable = ExpressionStatus == ModuleState.Uninitialized && moduleConfig.expression,
            eyeAvailable        = EyeStatus == ModuleState.Uninitialized && moduleConfig.eyes,
        };
        _logger.LogInformation("Got supported for module {module}. Expr: {expr} Eye: {eye}...",
            module.ModuleClassName, initPacket.expressionAvailable, initPacket.eyeAvailable);
        _sandboxServer.SendData(initPacket, port);
    }

    private async void HandleReplyInit(ReplyInitPacket reply, int port, int moduleIndex)
    {
        var module = AvailableSandboxModules[moduleIndex];
        var moduleConfig = await _moduleConfigurationService.GetInitializationConfig(module.ModuleMetadata.ModuleId);

        module.ModuleInformation.Name = reply.ModuleInformationName;
        module.SupportsEyeTracking = module.SupportsEyeTracking && reply.eyeSuccess;
        module.SupportsExpressionTracking =
            module.SupportsExpressionTracking && reply.expressionSuccess;

        _logger.LogInformation("Got init for module {module}. Eye: {eye} Expr: {expr}...",
            module.ModuleClassName, reply.eyeSuccess, reply.expressionSuccess);

        NoteInitReply();

        // Skip modules that failed to init anything
        if (!reply.eyeSuccess && !reply.expressionSuccess)
        {
            RefreshLoadedModulesUi();
            return;
        }

        if (EyeStatus != ModuleState.Active) EyeStatus = reply.eyeSuccess ? ModuleState.Active : ModuleState.Uninitialized;
        if (ExpressionStatus != ModuleState.Active) ExpressionStatus = reply.expressionSuccess ? ModuleState.Active : ModuleState.Uninitialized;

        module.ModuleInformation.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is not (nameof(ModuleMetadataInternal.Active)
                                       or nameof(ModuleMetadataInternal.UsingEye)
                                       or nameof(ModuleMetadataInternal.UsingExpression)))
            {
                return;
            }

            module.Status = module.ModuleInformation.Active ? ModuleState.Active : ModuleState.Idle;
            _sandboxServer.SendData(
                new EventStatusUpdatePacket
                {
                    ModuleState     = module.Status,
                    UsingEye        = module.ModuleInformation.UsingEye,
                    UsingExpression = module.ModuleInformation.UsingExpression,
                },
                port);
        };

        module.ModuleInformation.Active          = true;
        // I wouldn't otherwise check moduleConfig for this, but we can't trust modules for accuracy
        module.ModuleInformation.UsingEye        = moduleConfig.eyes && !AvailableSandboxModules.Any(m => m.ModuleInformation.UsingEye)        && reply.eyeSuccess;
        module.ModuleInformation.UsingExpression = moduleConfig.expression && !AvailableSandboxModules.Any(m => m.ModuleInformation.UsingExpression) && reply.expressionSuccess;
        module.ModuleInformation.StaticImages    = reply.IconDataStreams;

        _sendCoordinator.RegisterModule(port);
        EnsureModuleThreadStartedSandboxed(module);

        _logger.LogInformation("Tracking initialized via {module}", module.ModuleClassName);
        RefreshLoadedModulesUi();
    }

    private void NoteInitReply()
    {
        if (_pendingInits > 0)
        {
            _pendingInits--;
        }

        if (_pendingInits == 0)
        {
            _isInitializing = false;
        }
    }

    private void HandleReplyUpdate(ReplyUpdatePacket reply, int moduleIndex)
    {
        var module = AvailableSandboxModules[moduleIndex];
        if (module.Status != ModuleState.Active || !module.ModuleInformation.Active)
        {
            return;
        }

        if (module.ModuleInformation.UsingEye)
        {
            reply.UpdateGlobalEyeState();
        }
        if (module.ModuleInformation.UsingExpression)
        {
            reply.UpdateGlobalExpressionState();
        }
        reply.UpdateHeadState();

        _sendCoordinator.NotifyReply(module.SandboxProcessPort);
    }

    private async Task InitialiseSandboxesBase(IEnumerable<InstallableTrackingModule> modules)
    {
        foreach (var module in modules)
        {
            var dll = module.AssemblyLoadPath;
            var config = await _moduleConfigurationService.GetInitializationConfig(module.ModuleId);
            if (config is { expression: false, eyes: false }) continue;
            
            try
            {
                var sandboxProcess = Process.Start(new ProcessStartInfo(
                    _sandboxProcessPath,
                    $"--port {_sandboxServer.Port} --module-path \"{dll}\" --parent-pid {Environment.ProcessId}")
                {
#if !DEBUG
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
#else
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
#endif
                });

#if DEBUG
                new Thread(() =>
                {
                    var output = sandboxProcess.StandardOutput.ReadToEnd()
                               + sandboxProcess.StandardError.ReadToEnd();
                    sandboxProcess.WaitForExit();
                    Debug.WriteLine(output);
                }).Start();
#endif

                var runtimeInfo = new ModuleRuntimeInfo
                {
                    SandboxProcessPID  = sandboxProcess.Id,
                    SandboxProcessPort = -1,
                    SandboxModulePath  = dll,
                    IsActive           = true,
                    Process            = sandboxProcess,
                    ModuleClassName    = Path.GetFileNameWithoutExtension(dll),
                    ModuleInformation  = new(),
                    ModuleMetadata     = module,
                    EventBus           = new(),
                };
                lock (AvailableSandboxModules)
                {
                    _logger.LogDebug("Started sandbox process with dll {dllPath}", dll);
                    AvailableSandboxModules.Add(runtimeInfo);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("{error} Failed to start sandbox process for {path}. Skipping...", e.Message, dll);
            }
        }
    }

    private void EnsureModuleThreadStartedSandboxed(ModuleRuntimeInfo module)
    {
        if (_moduleThreads.Any(existing =>
                existing.SandboxProcessPID  == module.SandboxProcessPID &&
                existing.SandboxProcessPort == module.SandboxProcessPort))
        {
            return;
        }

        var port = module.SandboxProcessPort;
        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            _logger.LogDebug("Starting thread for {module}", module.GetType().Name);
            var updatePacket = new EventUpdatePacket();
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(10); // 100Hz
                _sandboxServer.SendData(updatePacket, port);
            }
            _logger.LogDebug("Thread for {module} ended", module.GetType().Name);
        });
        thread.Start();

        module.UpdateCancellationToken = cts;
        module.UpdateThread = thread;
        _moduleThreads.Add(module);
    }

    private void AttemptSandboxedModuleInitialize(ModuleRuntimeInfo module)
    {
        var packet = new EventInitGetSupported();

        if (module.SandboxProcessPID != -1 && module.SandboxProcessPort > 0)
        {
            _sandboxServer.SendData(packet, module.SandboxProcessPort);
        }
        else
        {
            module.EventBus.Enqueue(new QueuedPacket
            {
                packet = packet,
                destinationPort = module.SandboxProcessPort,
            });
        }
    }

    private async Task<bool> TeardownModuleSandboxed(ModuleRuntimeInfo module)
    {
        _logger.LogInformation("Tearing down {module} ", module.ModuleClassName);

        _sendCoordinator.UnregisterModule(module.SandboxProcessPort);
        _sandboxServer.SendData(new EventTeardownPacket(), module.SandboxProcessPort);

        if (module.UpdateCancellationToken != null)
        {
            await module.UpdateCancellationToken.CancelAsync();
        }

        if (!(module.Process?.HasExited ?? true))
        {
            _logger.LogDebug("Module process has not yet exited");
            try
            {
                if (!(module.Process?.WaitForExit(200) ?? false))
                {
                    _logger.LogDebug("Module {id} didn't exit gracefully. Forcing kill...", module.Process?.Id ?? -1);
                    module.Process?.Kill(entireProcessTree: true);

                    if (!(module.Process?.WaitForExit(2000) ?? false))
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // taskkill /F /T has a higher success rate than Process.Kill for stubborn processes.
                            using var killer = Process.Start(new ProcessStartInfo
                            {
                                FileName = "taskkill",
                                Arguments = $"/F /T /PID {module.Process.Id}",
                                CreateNoWindow = true,
                                UseShellExecute = false,
                            });
                            killer?.WaitForExit(2000);
                        }
                        else
                        {
                            _logger.LogCritical("Process {id} is a zombie or stuck in Kernel I/O. Manual intervention required.", module.Process.Id);
                        }
                        return false;
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // OpenProcessEx can fail with ACCESS_DENIED (e.g. process has higher privileges).
                _logger.LogError($"Tried killing process with PID {module.Process.Id}. Got win32 error ({ex})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tried killing process with PID {module.Process.Id}. Got exception ({ex.HResult}) {ex.Message}");
            }
        }

        if (module.UpdateThread?.IsAlive ?? false)
        {
            var moduleName = module.ModuleInformation?.Name ?? module.ModuleClassName ?? "Unknown";
            _logger.LogDebug("Waiting for {module}'s thread to join...", moduleName);
            module.UpdateThread?.Join(500);
        }

        return true;
    }
}
