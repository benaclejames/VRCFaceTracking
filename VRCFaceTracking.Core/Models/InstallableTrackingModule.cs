using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    [JsonIgnore]
    public string AssemblyLoadPath
    {
        get; set;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}