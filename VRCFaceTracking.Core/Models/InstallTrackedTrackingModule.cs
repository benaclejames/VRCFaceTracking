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
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}