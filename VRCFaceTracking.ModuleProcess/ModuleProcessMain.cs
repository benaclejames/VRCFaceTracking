using System.CommandLine;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;
using Windows.System;
using System.Collections.Specialized;
using System.Text;
using VRCFaceTracking.Core.Library;
using System.Runtime.CompilerServices;
using VRCFaceTracking.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace VRCFaceTracking.ModuleProcess;

public class ModuleProcessMain
{
    // How long in seconds we should wait for a connection to be established before giving up
    private const int CONNECTION_TIMEOUT = 30;
    private static bool WaitForPackets = true;
    public static ModuleAssembly DefModuleAssembly;
    public static ILoggerFactory? LoggerFactory;
    public static ILogger<ModuleProcessMain> Logger;
    public static VrcftSandboxClient Client;

    private static Queue<IpcPacket> _packetsToSend = new ();

    private static object _callbackLock = new ();
    private static bool _shouldCallReceive = false;
    public static void QueueReceiveEvent()
    {
        lock ( _callbackLock )
        {
            _shouldCallReceive = true;
        }
    }

    public static int Main(string[] args)
    {
        try
        {
            if ( args.Length < 1 )
            {
                // Not enough arguments
                return ModuleProcessExitCodes.INVALID_ARGS;
            }

            var portOption = new Option<int?>(
            name: "--port",
            description: "The UDP port the VRCFT server is running on.");
            var modulePathOption = new Option<string?>(
            name: "--module-path",
            description: "The path to the module to load.");

            var rootCommand = new RootCommand("VRCFT Sandbox Module");
            rootCommand.AddOption(portOption);
            rootCommand.AddOption(modulePathOption);

            rootCommand.SetHandler((modulePath, port) =>
            {
                VrcftMain(modulePath!, port ?? 0);
            }, modulePathOption, portOption);

            return rootCommand.Invoke(args);
        }
        catch ( Exception ex )
        {
            // So that we can catch errors
            Logger.LogCritical($"{ex.Message}:\n{ex.StackTrace}");
            Logger.LogCritical($"{ex.Message}");
#if DEBUG
            Console.ReadKey();
            Console.ReadLine();
#endif
            return ModuleProcessExitCodes.EXCEPTION_CRASH;
        }
        finally
        {
            Client?.Dispose();
        }
    }

    static int VrcftMain(string modulePath, int serverPortNumber)
    {
        // Give the main process enough time to add the module to the list before we begin sending data
        Thread.Sleep(50);

        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging((loggingBuilder) => loggingBuilder
                .ClearProviders()
                .AddDebug()
                .AddConsole()
                // .AddSentry(o =>
                //     o.Dsn =
                //     "https://444b0799dd2b670efa85d866c8c12134@o4506152235237376.ingest.us.sentry.io/4506152246575104")
                .AddProvider(new ProxyLoggerProvider(DispatcherQueue.GetForCurrentThread()))
            )
        .BuildServiceProvider();

        LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Logger = LoggerFactory.CreateLogger<ModuleProcessMain>();

        // A module process will connect to a given port number first. We try connecting to the server for 30 seconds, then give up, returning an error code in the process.
        Stopwatch stopwatch = new Stopwatch(); // For timeout
        Client = new VrcftSandboxClient(serverPortNumber, LoggerFactory);

        // Bind the log function so that we can forward log messages to VRCFT's main process
        ProxyLogger.OnLog += (LogLevel level, string msg) =>
        {
            var pkt = new EventLogPacket(level, msg);
            Client.SendData(pkt);
        };

        // Try loading the module
        DefModuleAssembly = new ModuleAssembly(Logger, LoggerFactory, modulePath);
        DefModuleAssembly.TryLoadAssembly();

        Client.OnReceiveShouldBeQueued += QueueReceiveEvent;
        Client.OnPacketReceivedCallback += (in IpcPacket packet) => {
            // Reset the timeout
            stopwatch.Restart();

            // Handle packets
            switch ( packet.GetPacketType() )
            {
                case IpcPacket.PacketType.EventGetSupported:
                    {
                        var result = DefModuleAssembly.TrackingModule.Supported;
                        var pkt = new ReplySupportedPacket()
                        {
                            eyeAvailable        = result.SupportsEye,
                            expressionAvailable = result.SupportsExpression
                        };
                        _packetsToSend.Enqueue(pkt);
                        break;
                    }
                case IpcPacket.PacketType.EventInit:
                    {
                        var pkt = (EventInitPacket) packet;

                        bool eyeSuccess, expressionSuccess;
                        try
                        {
                            (eyeSuccess, expressionSuccess) = DefModuleAssembly.TrackingModule.Initialize(pkt.eyeAvailable, pkt.expressionAvailable);
                        }
                        catch ( MissingMethodException )
                        {
                            Logger.LogError("{moduleName} does not properly implement ExtTrackingModule. Skipping.", DefModuleAssembly.GetType().Name);
                            return;
                        } catch ( Exception e )
                        {
                            Logger.LogError("Exception initializing {module}. Skipping. {e}", DefModuleAssembly.GetType().Name, e);
                            return;
                        }
                        var pktNew = new ReplyInitPacket()
                        {
                            eyeSuccess              = eyeSuccess,
                            expressionSuccess       = expressionSuccess,
                            ModuleInformationName   = DefModuleAssembly.TrackingModule.ModuleInformation.Name,
                            IconDataStreams         = DefModuleAssembly.TrackingModule.ModuleInformation.StaticImages
                        };
                        _packetsToSend.Enqueue(pktNew);
                        break;
                    }

                case IpcPacket.PacketType.EventTeardown:
                    {
                        DefModuleAssembly.TrackingModule.Teardown();
                        
                        // Tell VRCFT that we have shut down successfully (otherwise VRCFT will terminate this process)
                        var pkt = new ReplyTeardownPacket();
                        _packetsToSend.Enqueue(pkt);

                        // Shut down the event loop
                        WaitForPackets = false;
                        break;
                    }

                case IpcPacket.PacketType.EventUpdate:
                    {
                        // Logger.LogDebug("EventUpdate");
                        DefModuleAssembly.TrackingModule.Update();
                        var pkt = new ReplyUpdatePacket();
                        _packetsToSend.Enqueue(pkt);
                        break;
                    }

                case IpcPacket.PacketType.EventUpdateStatus:
                    {
                        var pkt = (EventStatusUpdatePacket) packet;
                        DefModuleAssembly.TrackingModule.Status = pkt.ModuleState;

                        break;
                    }
            }

        };
        // Start the connection
        Client.Connect();
        Logger.LogInformation("Initializing {module}", DefModuleAssembly.Assembly.ToString());

        stopwatch.Start();
        
        // Loop infinitely while we wait for commands
        while ( WaitForPackets )
        {
            if (stopwatch.Elapsed.TotalSeconds > CONNECTION_TIMEOUT)
            {
                Client.Close();
                return ModuleProcessExitCodes.NETWORK_CONNECTION_TIMED_OUT;
            }

            // Send packets in loop
            while (_packetsToSend.TryDequeue(out IpcPacket pkt))
            {
                Client.SendData(pkt);
            }

            // Tell the client to receive data
            if ( _shouldCallReceive )
            {
                Client.ReceivePackets();
            }

            Thread.Sleep(1);
        }

        return ModuleProcessExitCodes.OK;
    }
}