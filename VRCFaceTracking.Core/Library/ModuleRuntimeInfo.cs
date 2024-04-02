using System.Runtime.Loader;

namespace VRCFaceTracking.Core.Library;

public record ModuleRuntimeInfo(ExtTrackingModule Module, AssemblyLoadContext AssemblyLoadContext, CancellationTokenSource UpdateCancellationToken, Thread UpdateThread)
{
    public readonly ExtTrackingModule Module = Module;
    public readonly AssemblyLoadContext AssemblyLoadContext = AssemblyLoadContext;
    public readonly CancellationTokenSource UpdateCancellationToken = UpdateCancellationToken;
    public readonly Thread UpdateThread = UpdateThread;
}