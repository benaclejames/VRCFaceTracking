using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core.Contracts.Services;

public abstract partial class ParameterOutputService : ObservableObject
{
    [ObservableProperty]
    [property: SavedSetting("OSCInPort", 9001)] 
    public int inPort;

    [ObservableProperty]
    [property: SavedSetting("OSCOutPort", 9000)]
    public int outPort;

    [ObservableProperty]
    [property: SavedSetting("OSCAddress", "127.0.0.1")]
    public string destinationAddress;

    [ObservableProperty] public bool isConnected;

    public Action OnMessageDispatched;
    public Action<OscMessage> OnMessageReceived;
    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded;
    

    public abstract Task SaveSettings();
    public abstract void Send(OscMessage msg);
    public abstract Task<(bool, bool)> InitializeAsync();
    public abstract void Teardown();
}
