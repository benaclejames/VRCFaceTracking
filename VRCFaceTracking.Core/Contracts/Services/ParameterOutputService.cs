using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core.Contracts.Services;

public abstract class ParameterOutputService : INotifyPropertyChanged
{
    private int _inPort, _outPort;
    private string _destinationAddress;
    private bool _isConnected;
    
    [SavedSetting("OSCInPort", 9001)]
    public int InPort
    {
        get => _inPort;
        set => SetField(ref _inPort, value);
    }

    [SavedSetting("OSCOutPort", 9000)]
    public int OutPort
    {
        get => _outPort;
        set => SetField(ref _outPort, value);
    }
    
    [SavedSetting("OSCAddress", "127.0.0.1")]
    public string DestinationAddress
    {
        get => _destinationAddress;
        set => SetField(ref _destinationAddress, value);
    }
    
    public bool IsConnected
    {
        get => _isConnected;
        protected set => SetField(ref _isConnected, value);
    }

    public Action OnMessageDispatched;
    public Action<OscMessage> OnMessageReceived;
    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded;
    

    public abstract Task SaveSettings();
    public abstract void Send(OscMessage msg);
    public abstract Task<(bool, bool)> InitializeAsync();
    public abstract void Teardown();
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
