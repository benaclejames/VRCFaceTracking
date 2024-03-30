using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.Models;

public partial class OscTarget : ObservableObject, IOscTarget
{
    private readonly ILocalSettingsService _localSettingsService;
    
    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] [property: SavedSetting("OSCInPort", 9001)]
    private int _inPort;

    [ObservableProperty] [property: SavedSetting("OSCOutPort", 9000)]
    private int _outPort;

    [ObservableProperty] [property: SavedSetting("OSCAddress", "127.0.0.1")]
    private string _destinationAddress;
    
    public OscTarget(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
        
        PropertyChanged += (_, _) => _localSettingsService.Save(this);
    }
}