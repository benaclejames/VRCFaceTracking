using System.Diagnostics;
using System.Runtime.Loader;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Library;

public class ModuleRuntimeInfo
{
    // @NOTE: The following 4 properties should all be removed by the time sandboxing is done

#if true

    public ExtTrackingModule Module;
    public AssemblyLoadContext AssemblyLoadContext;
    public CancellationTokenSource UpdateCancellationToken;
    public Thread UpdateThread;

#endif

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
    /// <summary>
    /// The module's retreived metadata
    /// </summary>
    public ModuleMetadata ModuleInformation;
    /// <summary>
    /// Module status
    /// </summary>
    public ModuleState Status = ModuleState.Uninitialized;
    /// <summary>
    /// The class name of the module. Retrieved through a metadata packet.
    /// </summary>
    public string ModuleClassName;

    /// <summary>
    /// Queue of packets to send
    /// </summary>
    public Queue<QueuedPacket> EventBus;
}

public struct QueuedPacket
{
    public IpcPacket packet;
    public int destinationPort;
}