using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models;

public partial class ModuleConfigEntry : ObservableObject
{
    private readonly IModuleConfigurationService _configService;
    private readonly Guid _moduleId;

    public string ModuleName { get; }
    public string AuthorName { get; }
    public string Version { get; }

    [ObservableProperty] private bool _eyesEnabled;
    [ObservableProperty] private bool _expressionEnabled;

    public ModuleConfigEntry(
        InstallableTrackingModule module,
        bool eyes,
        bool expression,
        IModuleConfigurationService configService)
    {
        _configService = configService;
        _moduleId = module.ModuleId;
        ModuleName = module.ModuleName;
        AuthorName = module.AuthorName;
        Version = module.Version;
        _eyesEnabled = eyes;
        _expressionEnabled = expression;
    }

    partial void OnEyesEnabledChanged(bool value) => _ = Persist();
    partial void OnExpressionEnabledChanged(bool value) => _ = Persist();

    private Task Persist() =>
        _configService.SetInitializationConfig(_moduleId, EyesEnabled, ExpressionEnabled);
}