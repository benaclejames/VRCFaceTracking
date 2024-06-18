using System.CommandLine;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;
using Windows.System;

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
        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging((loggingBuilder) => loggingBuilder
                .ClearProviders()
                .AddDebug()
                .AddConsole()
                // .AddSentry(o =>
                //     o.Dsn =
                //     "https://444b0799dd2b670efa85d866c8c12134@o4506152235237376.ingest.us.sentry.io/4506152246575104")
                // .AddProvider(new OutputLogProvider(DispatcherQueue.GetForCurrentThread()))
                // .AddProvider(new LogFileProvider())
            )
        .BuildServiceProvider();

        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<ModuleProcessMain>();

        logger.LogInformation($"Module path: {modulePath} ; Got port {serverPortNumber}");

        // A module process will connect to a given port number first. We try connecting to the server for 30 seconds, then give up, returning an error code in the process.
        Stopwatch stopwatch = new Stopwatch();
        VrcftSandboxClient client = new VrcftSandboxClient(serverPortNumber, loggerFactory);
        // Reset the timeout
        client.OnPacketReceivedCallback += () => {
            stopwatch.Restart();
        };
        client.Connect();

        stopwatch.Start();
        
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
