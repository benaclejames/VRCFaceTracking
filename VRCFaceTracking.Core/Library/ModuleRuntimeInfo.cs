using System.Runtime.Loader;

namespace VRCFaceTracking.Core.Library;

public struct ModuleRuntimeInfo
{
    public ExtTrackingModule Module;
    public AssemblyLoadContext AssemblyLoadContext;
    public CancellationTokenSource UpdateCancellationToken;
    public Thread UpdateThread;
}