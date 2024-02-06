using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core.Services;

public abstract class ParameterOutputService : INotifyPropertyChanged
{
    public int InPort, OutPort;
    public string DestinationAddress;

    public bool IsConnected;

    public Action OnMessageDispatched;
    public Action<OscMessage> OnMessageReceived;

    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded = (_, _) => { };
    public event PropertyChangedEventHandler PropertyChanged;

    public abstract Task SaveSettings();
    public abstract Task LoadSettings();
    public abstract void Send(OscMessage msg);
    public abstract Task<(bool, bool)> InitializeAsync();
    public abstract void Teardown();

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
     => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
