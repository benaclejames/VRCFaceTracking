using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Sandboxing;
public class VrcftSandboxServer
{
    private static readonly Random Random = new ();

    private string _moduleServerId;

    // View into the sub-process' newest packet
    private IpcPacket _stagedPacket = new();
    private NamedPipeServerStream _namedPipeServer;

    public VrcftSandboxServer(string targetNamedPipe)
    {
        _moduleServerId = $"{targetNamedPipe}_moduleId{Random.NextInt64()}";
        _namedPipeServer = new NamedPipeServerStream(_moduleServerId, PipeDirection.InOut, 1);
    }

    /// <summary>
    /// Whether the server is running and connected
    /// </summary>
    public bool IsServerRunning => _namedPipeServer?.IsConnected ?? false;
}
