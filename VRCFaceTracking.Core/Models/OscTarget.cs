using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models;

public partial class OscTarget : ObservableObject, IOscTarget
{
    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] [property: SavedSetting("OSCInPort", 9001)]
    private int _inPort;

    [ObservableProperty] [property: SavedSetting("OSCOutPort", 9000)]
    private int _outPort;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(InPort))] [NotifyPropertyChangedFor(nameof(OutPort))]
    [property: SavedSetting("OSCAddress", "127.0.0.1")]
    private string _destinationAddress;
    
    public OscTarget(ILocalSettingsService localSettingsService)
    {
        PropertyChanged += (_, _) => localSettingsService.Save(this);
    }
}