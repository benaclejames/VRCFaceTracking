using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using VRCFaceTracking.Activation;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Models;
using VRCFaceTracking.Notifications;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;
using VRCFaceTracking.Views;

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
    
    private ILogger _logger;

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

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDispatcherService, DispatcherService>();

            // Core Services
            services.AddSingleton<IIdentityService, IdentityService>();
            services.AddSingleton<ModuleInstaller>();
            services.AddSingleton<IModuleDataService, ModuleDataService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IOSCService, OscMain>();
            services.AddSingleton<IMainService, MainStandalone>();
            services.AddSingleton<ConfigParser>();
            services.AddSingleton<UnifiedTracking>();
            services.AddSingleton<ILibManager, UnifiedLibManager>();

            // Views and ViewModels
            services.AddTransient<ModuleRegistryViewModel>();
            services.AddTransient<ModuleRegistryPage>();
            services.AddTransient<ParameterViewModel>();
            services.AddTransient<ParametersViewModel>();
            services.AddTransient<ParametersPage>();
            services.AddTransient<OutputViewModel>();
            services.AddTransient<OutputPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<UnifiedTrackingMutator>();
            services.AddSingleton<RiskySettingsViewModel>();
            services.AddTransient<OscViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddSingleton<IAvatarInfo, AvatarViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();
        
        // Check for a "reset" file in the root of the app directory. If one is found, wipe all files from inside it
        // and delete the file.
        var resetFile = Path.Combine(Utils.PersistentDataDirectory, "reset");
        if (File.Exists(resetFile))
        {
            foreach (var file in Directory.GetFiles(Utils.PersistentDataDirectory))
            {
                File.Delete(file);
            }
        }

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

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unhandled exception");
        _logger.LogCritical("Stacktrace: {0}", e.Exception.StackTrace);
        _logger.LogCritical("Inner exception: {0}", e.Exception.InnerException);
        _logger.LogCritical("Message: {0}", e.Exception.Message);
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
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
