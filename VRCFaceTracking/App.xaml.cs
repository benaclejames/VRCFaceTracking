using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Sentry.Protocol;
using VRCFaceTracking.Activation;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Models;
using VRCFaceTracking.Notifications;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;
using VRCFaceTracking.Views;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace VRCFaceTracking;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }
    
    private ILogger? _logger;

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();
        
        // Check for a "reset" file in the root of the app directory. If one is found, wipe all files from inside it
        // and delete the file.
        var resetFile = Path.Combine(Utils.PersistentDataDirectory, "reset");
        if (File.Exists(resetFile))
        {
            // Delete everything including files and folders in Utils.PersistentDataDirectory
            foreach (var file in Directory.EnumerateFiles(Utils.PersistentDataDirectory, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
        }


        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
            logging.AddProvider(new OutputLogProvider(DispatcherQueue.GetForCurrentThread()));
            logging.AddProvider(new LogFileProvider());
        }).
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddTransient<GithubService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDispatcherService, DispatcherService>();

            // Core Services
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddSingleton<ModuleInstaller>();
            services.AddSingleton<IModuleDataService, ModuleDataService>();
            services.AddTransient<IFileService, FileService>();
            services.AddSingleton<OscQueryService>();
            services.AddSingleton<QueryRegistrar>();
            services.AddSingleton<IMainService, MainStandalone>();
            services.AddTransient<AvatarConfigParser>();
            services.AddTransient<OscQueryConfigParser>();
            services.AddSingleton<UnifiedTracking>();
            services.AddSingleton<ILibManager, UnifiedLibManager>();
            services.AddTransient<OpenVRService>();
            services.AddSingleton<IOscTarget, OscTarget>();
            services.AddSingleton<HttpHandler>();

            // Views and ViewModels
            services.AddTransient<ModuleRegistryViewModel>();
            services.AddTransient<ModuleRegistryPage>();
            services.AddTransient<ParameterViewModel>();
            services.AddTransient<ParametersViewModel>();
            services.AddTransient<ParametersPage>();
            services.AddTransient<OutputViewModel>();
            services.AddTransient<OutputPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<UnifiedTrackingMutator>();
            services.AddSingleton<RiskySettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddSingleton<OscSendService>();
            services.AddSingleton<OscRecvService>();
            services.AddSingleton<ParameterSenderService>();
            
            services.AddHostedService<ParameterSenderService>(provider => provider.GetService<ParameterSenderService>());
            services.AddHostedService<OscRecvService>(provider => provider.GetService<OscRecvService>());

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();
        
        var logBuilder = App.GetService<ILoggerFactory>();
        _logger = logBuilder.CreateLogger("App");

        // Kill any other instances of VRCFaceTracking.exe
        foreach (var proc in Process.GetProcessesByName("VRCFaceTracking"))
        {
            if (proc.Id != Process.GetCurrentProcess().Id)
            {
                proc.Kill();
            }
        }

        App.GetService<IAppNotificationService>().Initialize();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        SentrySdk.Init(o =>
        {
            o.Dsn = "https://444b0799dd2b670efa85d866c8c12134@o4506152235237376.ingest.sentry.io/4506152246575104";
            o.TracesSampleRate = 1.0;
            o.AutoSessionTracking = true;
            #if DEBUG
            o.Environment = "debug";
            #else
            o.Environment = "release";
            #endif
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version != null)
            {
                o.Release = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        });
        Current.UnhandledException += ExceptionHandler;
        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
        await Host.StartAsync();
    }
    
    [HandleProcessCorruptedStateExceptions, SecurityCritical]
    internal void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        _logger?.LogError(e.Exception, "Unhandled exception");
        _logger?.LogCritical("Stacktrace: {0}", e.Exception.StackTrace);
        _logger?.LogCritical("Inner exception: {0}", e.Exception.InnerException);
        _logger?.LogCritical("Message: {0}", e.Exception.Message);
        
        // We need to hold the reference, because the Exception property is cleared when accessed.
        var exception = e.Exception;
        if (exception != null)
        {
            // Tells Sentry this was an Unhandled Exception
            exception.Data[Mechanism.HandledKey] = false;
            exception.Data[Mechanism.MechanismKey] = "Application.UnhandledException";
            SentrySdk.CaptureException(exception);
            // Make sure the event is flushed to disk or to Sentry
            SentrySdk.FlushAsync(TimeSpan.FromSeconds(3)).Wait();
        }
    }

    public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
    {
        if (!typeof(TEnum).GetTypeInfo().IsEnum)
        {
            throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
        }
        return (TEnum)Enum.Parse(typeof(TEnum), text);
    }
}
