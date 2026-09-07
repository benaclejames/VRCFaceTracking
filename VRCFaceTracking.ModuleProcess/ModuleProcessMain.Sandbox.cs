using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.ModuleProcess;

// Got sick of the sandbox IPC handling stuff being in main
public partial class ModuleProcessMain
{
    private const double CONNECTION_TIMEOUT = 60.0;

    public static VrcftSandboxClient Client;

    private static readonly Queue<IpcPacket> _packetsToSend = new();
    private static Timer? _connectionTimer;

    private static readonly object _callbackLock = new();
    private static bool _shouldCallReceive = false;

    public static void QueueReceiveEvent()
    {
        lock (_callbackLock)
        {
            _shouldCallReceive = true;
        }
    }

    private static void ConnectSandboxClient(int serverPortNumber, string modulePath)
    {
        Client = new VrcftSandboxClient(serverPortNumber, LoggerFactory);

        // Forward log messages from this process back to the VRCFT host.
        ProxyLogger.OnLog += (level, msg) => Client.SendData(new EventLogPacket(level, msg));

        Client.OnReceiveShouldBeQueued += QueueReceiveEvent;
        Client.OnPacketReceivedCallback += OnClientPacketReceived;

        Logger.LogInformation("Connecting to Sandbox Server");
        Client.Connect(modulePath);

        _connectionTimer = new Timer(_ =>
        {
            Logger.LogWarning("No packets received for {timeout}s, assuming connection lost", CONNECTION_TIMEOUT);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }, null, TimeSpan.FromSeconds(CONNECTION_TIMEOUT), Timeout.InfiniteTimeSpan);
    }

    private static void OnClientPacketReceived(in IpcPacket packet)
    {
        // Any packet resets the "connection lost" timer.
        _connectionTimer?.Change(TimeSpan.FromSeconds(CONNECTION_TIMEOUT), Timeout.InfiniteTimeSpan);

        switch (packet.GetPacketType())
        {
            case IpcPacket.PacketType.EventGetSupported:
                HandleGetSupported();
                break;

            case IpcPacket.PacketType.EventInit:
                HandleInit((EventInitPacket)packet);
                break;

            case IpcPacket.PacketType.EventTeardown:
                HandleTeardown();
                break;

            case IpcPacket.PacketType.EventUpdate:
                _packetsToSend.Enqueue(new ReplyUpdatePacket());
                break;

            case IpcPacket.PacketType.EventUpdateStatus:
                var updateStatus = (EventStatusUpdatePacket)packet;
                DefModuleAssembly.TrackingModule.Status = updateStatus.ModuleState;
                DefModuleAssembly.TrackingModule.ModuleInformation.UsingEye = updateStatus.UsingEye;
                DefModuleAssembly.TrackingModule.ModuleInformation.UsingExpression = updateStatus.UsingExpression;
                break;
        }
    }

    private static void HandleGetSupported()
    {
        var result = DefModuleAssembly.TrackingModule.Supported;
        _packetsToSend.Enqueue(new ReplySupportedPacket
        {
            eyeAvailable        = result.SupportsEye,
            expressionAvailable = result.SupportsExpression,
        });
    }

    private static void HandleInit(EventInitPacket pkt)
    {
        bool eyeSuccess, expressionSuccess;
        try
        {
            (eyeSuccess, expressionSuccess) = DefModuleAssembly.TrackingModule.Initialize(pkt.eyeAvailable, pkt.expressionAvailable);
        }
        catch (MissingMethodException)
        {
            Logger.LogError("{moduleName} does not properly implement ExtTrackingModule. Skipping.", DefModuleAssembly.GetType().Name);
            return;
        }
        catch (Exception e)
        {
            Logger.LogError("Exception initializing {module}. Skipping. {e}", DefModuleAssembly.GetType().Name, e);
            return;
        }

        DefModuleAssembly._updateCts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            while (!DefModuleAssembly._updateCts.IsCancellationRequested)
            {
                DefModuleAssembly.TrackingModule.Update();
            }
        })
        {
            // Background so the CLR can terminate even if the module blocks in native code (looking at you, Vive).
            IsBackground = true,
        };
        thread.Start();

        _packetsToSend.Enqueue(new ReplyInitPacket
        {
            eyeSuccess            = eyeSuccess,
            expressionSuccess     = expressionSuccess,
            ModuleInformationName = DefModuleAssembly.TrackingModule.ModuleInformation.Name,
            IconDataStreams       = DefModuleAssembly.TrackingModule.ModuleInformation.StaticImages,
        });
    }

    private static void HandleTeardown()
    {
        Logger.LogInformation("Received Teardown packet");
        DefModuleAssembly._updateCts?.Cancel();
        try
        {
            DefModuleAssembly.TrackingModule.Teardown();
        }
        catch (Exception e)
        {
            Logger.LogWarning("Tracking module failed to cleanly shut down.");
            Logger.LogError(e.ToString());
        }

        Logger.LogInformation("Cancelled Update Threads");

        // Ack teardown so VRCFT doesn't force-kill us, then flush.
        Client.SendData(new ReplyTeardownPacket());
        Client.SendAllPendingPackets();

        Logger.LogInformation("Sent teardown ACK");
        Environment.Exit(ModuleProcessExitCodes.OK);
    }

    private static void RunSandboxPumpLoop()
    {
        while (WaitForPackets && !cts.IsCancellationRequested)
        {
            while (_packetsToSend.TryDequeue(out var pkt))
            {
                if (pkt == null) continue;
                Client.SendData(pkt);
            }

            if (_shouldCallReceive)
            {
                Client.ReceivePackets();
            }

            Thread.Sleep(1);
        }

        _connectionTimer?.Dispose();
    }
}
