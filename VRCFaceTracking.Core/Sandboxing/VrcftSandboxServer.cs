using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Core.Sandboxing;
public class VrcftSandboxServer
{
    private static readonly Random Random = new ();

    private string _namedPipeString;

    // View into the sub-process' local data
    private SubprocessData _subprocessData = new();
    private NamedPipeServerStream _namedPipeServer;

    public VrcftSandboxServer(string targetNamedPipe)
    {
        _namedPipeString = $"{targetNamedPipe}_moduleId{Random.NextInt64()}";
        _namedPipeServer = new NamedPipeServerStream(targetNamedPipe, PipeDirection.InOut, 1);
    }

    /// <summary>
    /// Whether the server is running and connected
    /// </summary>
    public bool IsServerRunning => _namedPipeServer?.IsConnected ?? false;
}
