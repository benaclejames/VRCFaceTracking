using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.mDNS;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services)
    {
        var outputLogProvider = new OutputLogProvider();
        services.AddSingleton(outputLogProvider);
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddConsole();
            logging.AddDebug();
            logging.AddProvider(outputLogProvider);
        });

        services.AddTransient<GithubService>();
        services.AddTransient<IFileService, FileService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<IDispatcherService, DispatcherService>();
        services.AddTransient<IIdentityService, IdentityService>();
        
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<OpenVRService>();

        services.AddTransient<AvatarConfigParser>();
        services.AddTransient<OscQueryConfigParser>();
        services.AddSingleton<ModuleInstaller>();
        services.AddSingleton<IModuleDataService, ModuleDataService>();
        services.AddSingleton<OscQueryService>();
        services.AddSingleton<MulticastDnsService>();
        services.AddSingleton<IMainService, MainStandalone>();
        services.AddSingleton<UnifiedTracking>();
        services.AddSingleton<ILibManager, UnifiedLibManager>();
        services.AddSingleton<IOscTarget, OscTarget>();
        services.AddSingleton<OscSendService>();
        services.AddSingleton<OscRecvService>();
        services.AddSingleton<HttpHandler>();
        services.AddSingleton<ParameterSenderService>();
        services.AddSingleton<UnifiedTrackingMutator>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<OutputViewModel>();
        services.AddSingleton<ModuleRegistryViewModel>();
        services.AddSingleton<MutatorViewModel>();
        services.AddSingleton<RiskySettingsViewModel>();
        services.AddSingleton<SettingsViewModel>();

        services.AddTransient<ParameterViewModel>();
        services.AddTransient<ParametersViewModel>();

        services.AddHostedService(p => p.GetRequiredService<ParameterSenderService>());
        services.AddHostedService(p => p.GetRequiredService<OscRecvService>());
    }
}