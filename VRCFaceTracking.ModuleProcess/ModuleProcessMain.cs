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

namespace VRCFaceTracking.ModuleProcess;

public class ModuleProcessMain
{
    // How long in seconds we should wait for a connection to be established before giving up
    private const int CONNECTION_TIMEOUT = 30;

    public static int Main(string[] args)
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
                // .AddProvider(new LogFileProvider())
            )
        .BuildServiceProvider();

        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<ModuleProcessMain>();

        // A module process will connect to a given port number first. We try connecting to the server for 30 seconds, then give up, returning an error code in the process.
        Stopwatch stopwatch = new Stopwatch(); // For timeout
        VrcftSandboxClient client = new VrcftSandboxClient(serverPortNumber, loggerFactory);

        // Bind the log function so that we can forward log messages to VRCFT's main process
        ProxyLogger.OnLog += (LogLevel level, string msg) =>
        {
            var pkt = new EventLogPacket(level, msg);
            client.SendData(pkt);
        };

        // Try loading the module
        ModuleAssembly moduleAssembly = new ModuleAssembly(logger, modulePath);
        moduleAssembly.TryLoadAssembly();

        client.OnPacketReceivedCallback += (in IpcPacket packet) => {
            // Reset the timeout
            stopwatch.Restart();

            // Handle packets
            switch ( packet.GetPacketType() )
            {
                case IpcPacket.PacketType.EventInit:
                    {
                        var pkt = (EventInitPacket) packet;
                        var result = moduleAssembly.TrackingModule.Initialize(pkt.eyeAvailable, pkt.expressionAvailable);
                        logger.LogInformation("Eye: {eye} Expression: {expression}", result.eyeSuccess, result.expressionSuccess);
                        break;
                    }
            }

        };
        // Start the connection
        client.Connect();
        logger.LogInformation("Initializing {module}", moduleAssembly.Assembly.ToString());

        stopwatch.Start();
        
        // Loop infinitely while we wait for commands
        while ( true )
        {
            if (stopwatch.Elapsed.TotalSeconds > CONNECTION_TIMEOUT)
            {
                return ModuleProcessExitCodes.NETWORK_CONNECTION_TIMED_OUT;
            }

            Thread.Sleep(1);
        }

        return ModuleProcessExitCodes.OK;
    }
}
