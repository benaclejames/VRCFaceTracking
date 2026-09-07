using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models;

public partial class ModuleConfigEntry : ObservableObject
{
    private readonly IModuleConfigurationService? _configService;

    public Guid Id { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    [ObservableProperty]
    [property: SavedSetting("EyesEnabled", true)]
    private bool _eyesEnabled;

    [ObservableProperty]
    [property: SavedSetting("ExpressionEnabled", true)]
    private bool _expressionEnabled;

    public ModuleConfigEntry(
        TrackingModuleMetadata module,
        IModuleConfigurationService configService)
    {
        _configService = configService;
        Id = module.ModuleId;
        ModuleName = module.ModuleName;
        AuthorName = module.AuthorName;
        Version = module.Version;
    }

    partial void OnEyesEnabledChanged(bool value) => _ = Persist();
    partial void OnExpressionEnabledChanged(bool value) => _ = Persist();

    private Task Persist() =>
        _configService?.SaveModule(this) ?? Task.CompletedTask;
}
