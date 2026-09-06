using System.ComponentModel;
using Newtonsoft.Json;

namespace VRCFaceTracking.Core.Models;

public enum InstallState
{
    NotInstalled,
    Installed,
    Outdated,
    AwaitingRestart
}

public class InstallableTrackingModule : TrackingModuleMetadata, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public InstallState InstallationState
    {
        get; set;
    }
    
    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }

    private ModuleEnabledState _state = ModuleEnabledState.Enabled;

    /// <summary>
    /// The enabled state of this module. Changes take effect after VRCFT is restarted.
    /// The value is persisted separately and does not get written to module.json.
    /// </summary>
    [JsonIgnore]
    public ModuleEnabledState State
    {
        get => _state;
        set
        {
            if ( _state == value )
            {
                return;
            }

            _state = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }
    }

    private string _stateBadgeText = string.Empty;

    /// <summary>
    /// Localized display text for the module's enabled state shown in the module list.
    /// Shows "(old → new)" when the state has been changed and a restart is pending.
    /// </summary>
    [JsonIgnore]
    public string StateBadgeText
    {
        get => _stateBadgeText;
        set
        {
            if ( _stateBadgeText == value )
            {
                return;
            }

            _stateBadgeText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateBadgeText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasStateBadge)));
        }
    }

    /// <summary>
    /// Whether the module list should show a state badge (false for not-installed modules).
    /// </summary>
    [JsonIgnore]
    public bool HasStateBadge => !string.IsNullOrEmpty(_stateBadgeText);

    /// <summary>
    /// Updates the list badge text. The state-to-string conversion is delegated to
    /// <paramref name="localize"/> so this Core model stays free of UI dependencies.
    /// Shows "(old → new)" when the state has been changed and a restart is pending.
    /// </summary>
    public void UpdateStateBadge(ModuleEnabledState? applied, Func<ModuleEnabledState, string> localize)
    {
        var current = localize(State);
        StateBadgeText = applied.HasValue && applied.Value != State
            ? $"({localize(applied.Value)} → {current})"
            : $"({current})";
    }

    /// <summary>
    /// A stable identifier used to persist the enabled/disabled state of a module.
    /// Registry modules are keyed by their module id, legacy modules by their assembly path.
    /// </summary>
    [JsonIgnore]
    public string ModuleKey => ModuleId != Guid.Empty ? ModuleId.ToString() : AssemblyLoadPath;
}