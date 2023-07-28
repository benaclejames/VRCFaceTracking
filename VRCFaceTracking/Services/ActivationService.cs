﻿using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.Activation;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Views;

namespace VRCFaceTracking.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IOSCService _oscService;
    private readonly IMainService _mainService;
    private readonly IModuleDataService _moduleDataService;
    private readonly ModuleInstaller _moduleInstaller;
    private readonly ILibManager _libManager;
    private readonly ILogger _logger;
    private UIElement? _shell = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, 
        IEnumerable<IActivationHandler> activationHandlers, IThemeSelectorService themeSelectorService, IOSCService oscService,
        IMainService mainService, IModuleDataService moduleDataService, ModuleInstaller moduleInstaller, ILibManager libManager,
        ILoggerFactory loggerFactory)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _oscService = oscService;
        _mainService = mainService;
        _moduleDataService = moduleDataService;
        _moduleInstaller = moduleInstaller;
        _libManager = libManager;
        _logger = loggerFactory.CreateLogger("MainStandalone");
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
        await _oscService.InitializeAsync().ConfigureAwait(false);

        _logger.LogInformation("Initializing main service...");
        await _mainService.InitializeAsync().ConfigureAwait(false);

        // Before we initialize, we need to delete pending restart modules and check for updates for all our installed modules
        _logger.LogDebug("Checking for deletion requests for installed modules...");
        var needsDeleting = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules())
            .Where(m => m.InstallationState == InstallState.AwaitingRestart);
        foreach (var deleteModule in needsDeleting)
            _moduleInstaller.UninstallModule(deleteModule);
        
        _logger.LogInformation("Checking for updates for installed modules...");
        var localModules = _moduleDataService.GetInstalledModules().Where(m => m.ModuleId != Guid.Empty);
        var remoteModules = await _moduleDataService.GetRemoteModules();
        var outdatedModules = remoteModules.Where(rm => localModules.Any(lm => rm.ModuleId == lm.ModuleId && rm.Version != lm.Version));
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
