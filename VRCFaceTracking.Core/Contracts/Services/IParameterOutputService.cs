using System.ComponentModel;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core.Contracts.Services;

public interface IParameterOutputService : INotifyPropertyChanged
{
    public int InPort { get; set; }
    public int OutPort { get; set; }
    public string DestinationAddress { get; set; }

    public bool IsConnected { get; set; }

    public Action OnMessageDispatched { get; set; }
    public Action<OscMessage> OnMessageReceived { get; set; }

    public Action<IAvatarInfo, List<Parameter>> OnAvatarLoaded { get; set; }

    public Task SaveSettings();
    public Task LoadSettings();
    public void Send(OscMessage msg);
    public Task<(bool, bool)> InitializeAsync();
    public void Teardown();
}
