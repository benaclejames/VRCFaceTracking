namespace VRCFaceTracking.Contracts.Services;

/// <summary>
/// Integration with a PC VR runtime (SteamVR via OpenVR on desktop), used to register VRCFT to launch alongside it.
/// Platforms without such a runtime use <see cref="Services.NullVrRuntimeService"/>.
/// </summary>
public interface IVrRuntimeService
{
    bool IsInitialized { get; }

    bool AutoStart { get; set; }

    bool Initialize();

    void InitIfNotAlready();
}
