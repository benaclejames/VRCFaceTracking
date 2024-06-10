using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ModuleProcess;
public class VrcftSandboxClient
{
    // Named pipe local machine
    private const string NAMEDPIPE_MACHINE_ENDPOINT_LOCAL_MACHINE = ".";

    private readonly NamedPipeClientStream? _namedPipeClient = null;
    private bool _isConnectionOk = false;

    public VrcftSandboxClient(string targetNamedPipe)
    {
        _namedPipeClient = new NamedPipeClientStream(
            NAMEDPIPE_MACHINE_ENDPOINT_LOCAL_MACHINE,
            targetNamedPipe,
            PipeDirection.InOut,
            PipeOptions.None,
            TokenImpersonationLevel.Impersonation);


    }

    public bool IsConnected => _namedPipeClient != null && _isConnectionOk;

    public void Connect()
    {
        if (!IsConnected)
        {
            _namedPipeClient.Connect();
            _isConnectionOk = _namedPipeClient.IsConnected;
        }
    }

    private void SyncData(SubprocessData subprocessData)
    {

    }

    [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
    private static unsafe extern void CopyMemory(void* dest, void* src, int count);
}
