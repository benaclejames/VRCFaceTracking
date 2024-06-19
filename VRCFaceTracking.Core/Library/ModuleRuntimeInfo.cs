using System.Diagnostics;
using System.Runtime.Loader;

namespace VRCFaceTracking.Core.Library;

public struct ModuleRuntimeInfo
{
    public ExtTrackingModule Module;
    public AssemblyLoadContext AssemblyLoadContext;
    public CancellationTokenSource UpdateCancellationToken;
    public Thread UpdateThread;
    /// <summary>
    /// Whether the module is active and will receive update events.
    /// </summary>
    public bool IsActive;
    /// <summary>
    /// The UDP port the sandbox process associated with this application is on
    /// </summary>
    public int SandboxProcessPort;
    /// <summary>
    /// The PID of a sandbox process
    /// </summary>
    public int SandboxProcessPID;
    /// <summary>
    /// The path to the module the sandbox shall load
    /// </summary>
    public string SandboxModulePath;
    /// <summary>
    /// The process hosting the sandboxed module
    /// </summary>
    public Process Process;
}