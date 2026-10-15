using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRCFaceTracking.Core.Models;

public class InstallTrackedTrackingModule : INotifyPropertyChanged
{
    public TrackingModuleMetadata  TrackingModuleMetadata { get; set; }
    
    public InstallState InstallationState
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    private ModuleEnabledState _state = ModuleEnabledState.Enabled;

    /// <summary>
    /// The enabled state of this module. Changes take effect after VRCFT is restarted.
    /// </summary>
    public ModuleEnabledState State
    {
        get => _state;
        set
        {
            if (value != _state)
            {
                _state = value;
                OnPropertyChanged();
            }
        }
    }

    private string _stateBadgeText = string.Empty;

    /// <summary>
    /// Localized display text for the module's state, shown in the module list.
    /// Shows "(old → new)" when the state has been changed and a restart is pending.
    /// </summary>
    public string StateBadgeText
    {
        get => _stateBadgeText;
        set
        {
            if (value != _stateBadgeText)
            {
                _stateBadgeText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStateBadge));
            }
        }
    }

    public bool HasStateBadge => !string.IsNullOrEmpty(_stateBadgeText);

    /// <summary>
    /// Stable identifier used to persist the module's state.
    /// </summary>
    public string ModuleKey =>
        TrackingModuleMetadata is InstallableTrackingModule installed && installed.ModuleId == Guid.Empty
            ? installed.AssemblyLoadPath
            : TrackingModuleMetadata.ModuleId.ToString();

    /// <summary>
    /// Updates the list badge text. The state-to-string conversion is delegated to
    /// <paramref name="localize"/> so this Core model stays free of UI dependencies.
    /// </summary>
    public void UpdateStateBadge(ModuleEnabledState? applied, Func<ModuleEnabledState, string> localize)
    {
        var current = localize(State);
        StateBadgeText = applied.HasValue && applied.Value != State
            ? $"({localize(applied.Value)} → {current})"
            : $"({current})";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}