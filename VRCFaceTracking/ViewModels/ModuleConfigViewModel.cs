using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public partial class ModuleConfigViewModel : ObservableRecipient
{
    private readonly IModuleDataService _moduleDataService;
    private readonly IModuleConfigurationService _moduleConfigurationService;

    private readonly Dictionary<Guid, (bool Eyes, bool Expression)> _applied = new();

    public ObservableCollection<ModuleConfigEntry> Modules { get; } = new();

    public bool HasModules => Modules.Count > 0;

    [ObservableProperty] private bool _hasPendingChanges;

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
        foreach (var m in Modules)
        {
            m.PropertyChanged -= ModuleOnPropertyChanged;
        }
        Modules.Clear();
        _applied.Clear();

        foreach (var module in _moduleDataService.GetInstalledModules())
        {
            var entry = await _moduleConfigurationService.LoadModule(module.ModuleId);
            if (entry == null) continue;
            
            entry.PropertyChanged += ModuleOnPropertyChanged;
            _applied[entry.Id] = (entry.EyesEnabled, entry.ExpressionEnabled);
            Modules.Add(entry);
        }

        HasPendingChanges = false;
        OnPropertyChanged(nameof(HasModules));
    }

    public void MarkApplied()
    {
        foreach (var m in Modules)
        {
            _applied[m.Id] = (m.EyesEnabled, m.ExpressionEnabled);
        }
        HasPendingChanges = false;
    }

    private void ModuleOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ModuleConfigEntry)
        {
            return;
        }
        if (e.PropertyName != nameof(ModuleConfigEntry.EyesEnabled) &&
            e.PropertyName != nameof(ModuleConfigEntry.ExpressionEnabled))
        {
            return;
        }
        RecomputePending();
    }

    private void RecomputePending()
    {
        foreach (var m in Modules)
        {
            if (!_applied.TryGetValue(m.Id, out var baseline) ||
                baseline.Eyes != m.EyesEnabled ||
                baseline.Expression != m.ExpressionEnabled)
            {
                HasPendingChanges = true;
                return;
            }
        }
        HasPendingChanges = false;
    }
}
