using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core.Library;

public partial class UnifiedLibManager : ILibManager
{
    private readonly ILogger<UnifiedLibManager> _logger;
    private readonly ILogger _moduleLogger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDispatcherService _dispatcherService;
    private readonly IModuleDataService _moduleDataService;
    private readonly ILocalSettingsService _settingsService;
    private readonly SendCoordinator _sendCoordinator;

    public ObservableCollection<ModuleMetadataInternal> LoadedModulesMetadata { get; set; }

    public static ModuleState EyeStatus { get; private set; }
    public static ModuleState ExpressionStatus { get; private set; }

    public IReadOnlyDictionary<string, ModuleEnabledState> AppliedModuleStates { get; private set; } = new Dictionary<string, ModuleEnabledState>();

    // Sandbox stuff
    private readonly string _sandboxProcessPath;
    private readonly List<ModuleRuntimeInfo> AvailableSandboxModules = new();
    private readonly List<ModuleRuntimeInfo> _moduleThreads = new();
    private static VrcftSandboxServer _sandboxServer;
    private Dictionary<string, ModuleEnabledState> _moduleSettingsByPath = new();

    public UnifiedLibManager(ILoggerFactory factory, IDispatcherService dispatcherService, IModuleDataService moduleDataService, ILocalSettingsService settingsService, SendCoordinator sendCoordinator)
    {
        _loggerFactory = factory;
        _logger = factory.CreateLogger<UnifiedLibManager>();
        _moduleLogger = factory.CreateLogger("\0VRCFT\0");
        _dispatcherService = dispatcherService;
        _moduleDataService = moduleDataService;
        _settingsService = settingsService;
        _sendCoordinator = sendCoordinator;

        LoadedModulesMetadata = new ObservableCollection<ModuleMetadataInternal>();
        _sandboxProcessPath = Path.GetFullPath(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "VRCFaceTracking.ModuleProcess.exe" : "VRCFaceTracking.ModuleProcess");
        if ( !File.Exists(_sandboxProcessPath) )
        {
            // @TODO: Better error handling
            throw new FileNotFoundException($"Failed to find sandbox process at \"{_sandboxProcessPath}\"!");
        }

        // @TODO: Kill any lingering sub-modules to eliminate any conflicts
    }

    public async Task Initialize()
    {
        LoadedModulesMetadata.Clear();
        LoadedModulesMetadata.Add(new ModuleMetadataInternal
        { 
            Active = false,
            Name = "Initializing Modules..."
        });

        // Spawn sandbox server if it's null
        if (_sandboxServer == null )
        {
            // @TODO: Figure out an elegant way to ask the GUI for the ports the user assigned to the OSCTarget.
            var reservedPorts = new[] { 9000, 9001 };
            _sandboxServer = new VrcftSandboxServer(_loggerFactory, reservedPorts);
            _sandboxServer.OnPacketReceived += OnSandboxPacketReceived;
        }

        _logger.LogInformation("Starting initialization tracking");

        await TeardownAllModules();

        var modules = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules());

        // Load the per-module state settings. They are applied at startup, so changing a module's state
        // only takes effect after a restart.
        var allSettings = await _settingsService.ReadSettingAsync(Utils.ModuleStateSettingsKey, new Dictionary<string, ModuleEnabledState>());
        var modulesToLoad = new List<InstallableTrackingModule>();
        var settingsByPath = new Dictionary<string, ModuleEnabledState>();
        var appliedStates = new Dictionary<string, ModuleEnabledState>();
        foreach (var m in modules)
        {
            var state = allSettings.TryGetValue(m.ModuleKey, out var s) ? s : ModuleEnabledState.Enabled;
            settingsByPath[m.AssemblyLoadPath] = state;
            appliedStates[m.ModuleKey] = state;

            // Skip modules that have been disabled by the user as a whole
            if (state == ModuleEnabledState.Disabled)
            {
                continue;
            }

            modulesToLoad.Add(m);
        }

        _moduleSettingsByPath = settingsByPath;
        AppliedModuleStates = appliedStates;

        var modulePaths = modulesToLoad.Select(m => m.AssemblyLoadPath);

        AvailableSandboxModules.Clear();
        InitialiseSandboxesBaseOnPaths(modulePaths.ToArray());

        if (AvailableSandboxModules.Count > 0)
        {
            _logger.LogDebug("Initializing requested runtimes...");
            return;
        }

        _dispatcherService.Run(() =>
        {
            LoadedModulesMetadata.Clear();
            LoadedModulesMetadata.Add(new ModuleMetadataInternal
            {
                Active = false,
                Name = "No Modules Loaded",
            });
        });
        _logger.LogWarning("No modules loaded.");
    }

    // Signal all active modules to gracefully shut down their respective runtimes.
    public async Task TeardownAllModules()
    {
        _logger.LogInformation("Tearing down all modules...");

        foreach (var module in _moduleThreads)
        {
            await TryTeardownModule(module);
        }
        _moduleThreads.Clear();

        foreach (var module in AvailableSandboxModules)
        {
            await TryTeardownModule(module);
        }
        AvailableSandboxModules.Clear();

        EyeStatus = ModuleState.Uninitialized;
        ExpressionStatus = ModuleState.Uninitialized;
    }

    private async Task TryTeardownModule(ModuleRuntimeInfo module)
    {
        if (module == null || (module.Process?.HasExited ?? true))
        {
            return;
        }

        var success = false;
        try
        {
            success = await TeardownModuleSandboxed(module);
        }
        finally
        {
            if (!success)
            {
                var moduleName = module.ModuleInformation?.Name ?? module.ModuleClassName ?? "Unknown";
                _logger.LogWarning($"Module: {moduleName} failed to shut down. Killing its thread.");
                module.UpdateThread?.Interrupt();
            }
        }
    }
}
