using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Core.Contracts.Services;
public interface IOSCService
{
    int InPort { get; set; }
    int OutPort { get; set; }
    string Address { get; set; }
    
    Action OnMessageDispatched { get; set; }
    Action<OscMessageMeta> OnMessageReceived { get; set; }

    Task SaveSettings();
    Task LoadSettings();
    void Send(byte[] data, int length);
    Task<(bool, bool)> InitializeAsync();
}
