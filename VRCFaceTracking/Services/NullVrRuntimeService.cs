using VRCFaceTracking.Contracts.Services;

namespace VRCFaceTracking.Services;

// Default for platforms with no PC VR runtime to integrate with. Keeps the auto-start setting disabled in the UI.
public class NullVrRuntimeService : IVrRuntimeService
{
    public bool IsInitialized => false;

    public bool AutoStart
    {
        get => false;
        set { }
    }

    public bool Initialize() => false;

    public void InitIfNotAlready() { }
}
