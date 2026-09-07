using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.ModuleProcess;

public partial class ModuleProcessMain
{
    private static bool WaitForPackets = true;
    public static ModuleAssembly DefModuleAssembly;
    public static ILoggerFactory? LoggerFactory;
    public static ILogger<ModuleProcessMain> Logger;
    public static CancellationTokenSource cts = new();

    public static int Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Logger.LogInformation("Received SIGTERM");
            WaitForPackets = false;
            DefModuleAssembly._updateCts?.Cancel();
            cts.Cancel();
            cts.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
        };

        try
        {
            if (args.Length < 1)
            {
                return ModuleProcessExitCodes.INVALID_ARGS;
            }

            var portOption = new Option<int?>("--port")       { Description = "The UDP port the VRCFT server is running on." };
            var modulePathOption = new Option<string?>("--module-path") { Description = "The path to the module to load." };
            var parentPidOption = new Option<int?>("--parent-pid") { Description = "PID of the parent VRCFT process. Module process exits if parent dies." };

            var rootCommand = new RootCommand("VRCFT Sandbox Module");
            rootCommand.Options.Add(portOption);
            rootCommand.Options.Add(modulePathOption);
            rootCommand.Options.Add(parentPidOption);

            rootCommand.SetAction(parseResult =>
            {
                var modulePath = parseResult.GetValue(modulePathOption);
                var port = parseResult.GetValue(portOption);
                var parentPid = parseResult.GetValue(parentPidOption);
                VrcftMain(modulePath!, port ?? 0, parentPid);
                return 0;
            });

            return rootCommand.Parse(args).Invoke();
        }
        catch (Exception ex)
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

    private static int VrcftMain(string modulePath, int serverPortNumber, int? parentPid = null)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .ClearProviders()
                .AddDebug()
                .AddConsole()
                // .AddSentry(o =>
                //     o.Dsn =
                //     "https://444b0799dd2b670efa85d866c8c12134@o4506152235237376.ingest.us.sentry.io/4506152246575104")
                .AddProvider(new ProxyLoggerProvider())
            )
        .BuildServiceProvider();

        LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Logger = LoggerFactory!.CreateLogger<ModuleProcessMain>();

        StartParentWatchdog(parentPid);

        InitializeUnifiedTrackingToInvalidState();

        ConnectSandboxClient(serverPortNumber, modulePath);

        if (OperatingSystem.IsWindows())
        {
            Core.Utils.TimeBeginPeriod(1);
        }

        DefModuleAssembly = new ModuleAssembly(Logger, LoggerFactory, modulePath);
        Logger.LogInformation("Initializing {module}", Path.GetFileNameWithoutExtension(DefModuleAssembly.ModulePath));

        RunSandboxPumpLoop();

        DefModuleAssembly._updateCts.Cancel();

        if (OperatingSystem.IsWindows())
        {
            Core.Utils.TimeEndPeriod(1);
        }

        Environment.Exit(ModuleProcessExitCodes.OK);
        return ModuleProcessExitCodes.OK;
    }

    // Independent watchdog: if the parent VRCFT process exits, tear the module
    // down and exit so we don't linger as an orphaned process.
    private static void StartParentWatchdog(int? parentPid)
    {
        if (!parentPid.HasValue)
        {
            Logger.LogWarning("No parent pid provided. Lingering process detection is limited to existing timeouts and graceful shutdown");
            return;
        }

        var watchdogThread = new Thread(() =>
        {
            try
            {
                using var parent = Process.GetProcessById(parentPid.Value);
                parent.WaitForExit();
            }
            catch (Exception) { }

            // Give the module 10 seconds to shut down gracefully, then kill.
            var teardownThread = new Thread(() =>
            {
                try
                {
                    DefModuleAssembly?._updateCts?.Cancel();
                    DefModuleAssembly?.TrackingModule?.Teardown();
                }
                catch (Exception) { }
            })
            {
                IsBackground = true,
            };
            teardownThread.Start();
            teardownThread.Join(TimeSpan.FromSeconds(10));

            Process.GetCurrentProcess().Kill();
        })
        {
            IsBackground = true,
            Name = "ParentWatchdog",
        };
        watchdogThread.Start();
    }

    private static void InitializeUnifiedTrackingToInvalidState()
    {
        UnifiedTracking.Data = new()
        {
            Eye = new()
            {
                Left = new()
                {
                    Gaze = new(0xFFFFFFFF, 0xFFFFFFFF),
                    Openness = 0xFFFFFFFF,
                    PupilDiameter_MM = 0xFFFFFFFF,
                },
                Right = new()
                {
                    Gaze = new(0xFFFFFFFF, 0xFFFFFFFF),
                    Openness = 0xFFFFFFFF,
                    PupilDiameter_MM = 0xFFFFFFFF,
                },
                _maxDilation = 0xFFFFFFFF,
                _minDilation = 0xFFFFFFFF,
            },
        };
        for (var i = 0; i < (int)UnifiedExpressions.Max + 1; i++)
        {
            UnifiedTracking.Data.Shapes[i].Weight = 0xFFFFFFFF;
        }
    }
}
