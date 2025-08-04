using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.Activation;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Views;

namespace VRCFaceTracking.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly OscQueryService _parameterOutputService;
    private readonly IMainService _mainService;
    private readonly IModuleDataService _moduleDataService;
    private readonly ModuleInstaller _moduleInstaller;
    private readonly ILibManager _libManager;
    private readonly ILogger<ActivationService> _logger;
    private readonly OpenVRService _openVrService;
    private UIElement? _shell;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler, 
        IEnumerable<IActivationHandler> activationHandlers, 
        IThemeSelectorService themeSelectorService, 
        OscQueryService parameterOutputService,
        IMainService mainService, 
        IModuleDataService moduleDataService, 
        ModuleInstaller moduleInstaller, 
        ILibManager libManager,
        ILogger<ActivationService> logger,
        OpenVRService openVrService)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _parameterOutputService = parameterOutputService;
        _mainService = mainService;
        _moduleDataService = moduleDataService;
        _moduleInstaller = moduleInstaller;
        _libManager = libManager;
        _logger = logger;
        _openVrService = openVrService;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    private async Task InitializeAsync()
    {
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);

        await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        
        _logger.LogInformation("VRCFT Version {version} initializing...", Assembly.GetExecutingAssembly().GetName().Version);
        
        _logger.LogInformation("Initializing OSC...");
        await _parameterOutputService.InitializeAsync().ConfigureAwait(false);

        _logger.LogInformation("Initializing main service...");
        await _mainService.InitializeAsync().ConfigureAwait(false);
        
        _logger.LogInformation("Initializing OpenVR...");
        if (!_openVrService.Initialize())
        {
            _logger.LogWarning("Failed to initialize OpenVR during ActivationService startup. Skipping.");
        }

        // Before we initialize, we need to delete pending restart modules and check for updates for all our installed modules
        _logger.LogDebug("Checking for deletion requests for installed modules...");
        var needsDeleting = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules())
            .Where(m => m.InstallationState == InstallState.AwaitingRestart);
        foreach (var deleteModule in needsDeleting)
        {
            _moduleInstaller.UninstallModule(deleteModule);
        }

        _logger.LogInformation("Checking for updates for installed modules...");
        var localModules = _moduleDataService.GetInstalledModules().Where(m => m.ModuleId != Guid.Empty);
        var remoteModules = await _moduleDataService.GetRemoteModules();
        var outdatedModules = remoteModules.Where(rm => localModules.Any(lm =>
        {
            if (rm.ModuleId != lm.ModuleId) 
                return false;

            var remoteVersion = new Version(rm.Version);
            var localVersion = new Version(lm.Version);

            return remoteVersion.CompareTo(localVersion) > 0;
        }));
        foreach (var outdatedModule in outdatedModules)
        {
            _logger.LogInformation($"Updating {outdatedModule.ModuleName} from {localModules.First(rm => rm.ModuleId == outdatedModule.ModuleId).Version} to {outdatedModule.Version}");
            await _moduleInstaller.InstallRemoteModule(outdatedModule);
        }
        
        _logger.LogInformation("Initializing modules...");
        App.MainWindow.DispatcherQueue.TryEnqueue(() => _libManager.Initialize());
        
        await Task.CompletedTask;
    }
}
