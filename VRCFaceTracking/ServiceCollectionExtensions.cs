using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Services;
using VRCFaceTracking.Services.Logging;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        Core.ServiceCollectionExtensions.AddCommonServices(services);

        var logBuffer = new LogBufferProvider();
        services.AddSingleton(logBuffer);
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole();
            logging.AddDebug();
            logging.AddProvider(new OutputPageLogProvider());
            logging.AddProvider(new LogFileProvider());
            logging.AddProvider(logBuffer);

            logging.AddFilter<OutputPageLogProvider>(null, LogLevel.Information);
        });

        services.AddTransient<GithubService>();
        services.AddTransient<IFileService, FileService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<IDispatcherService, DispatcherService>();
        services.AddTransient<IIdentityService, IdentityService>();
        
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<OpenVRService>();
        services.AddSingleton<IModuleDataService, ModuleDataService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<OutputViewModel>();
        services.AddSingleton<ModuleRegistryViewModel>();
        services.AddSingleton<MutatorViewModel>();
        services.AddSingleton<RiskySettingsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddTransient<ParameterViewModel>();
        services.AddTransient<ParametersViewModel>(); 
    }
}