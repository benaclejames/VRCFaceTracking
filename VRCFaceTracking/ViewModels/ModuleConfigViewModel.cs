using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public class ModuleConfigViewModel : ObservableRecipient
{
    private readonly IModuleDataService _moduleDataService;
    private readonly IModuleConfigurationService _moduleConfigurationService;

    public ObservableCollection<ModuleConfigEntry> Modules { get; } = new();

    public bool HasModules => Modules.Count > 0;

    public ModuleConfigViewModel(
        IModuleDataService moduleDataService,
        IModuleConfigurationService moduleConfigurationService)
    {
        _moduleDataService = moduleDataService;
        _moduleConfigurationService = moduleConfigurationService;

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Modules.Clear();
        foreach (var module in _moduleDataService.GetInstalledModules())
        {
            var (eyes, expression) = await _moduleConfigurationService.GetInitializationConfig(module.ModuleId);
            Modules.Add(new ModuleConfigEntry(module, eyes, expression, _moduleConfigurationService));
        }
        OnPropertyChanged(nameof(HasModules));
    }
}
