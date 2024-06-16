using System.Net.Sockets;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.ModuleProcess;

public class ModuleProcessMain
{
    public static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            // Not enough arguments
            return ModuleProcessExitCodes.INVALID_ARGS;
        }

        int serverPortNumber = 0;
        if ( !int.TryParse(args[0], out serverPortNumber) )
        {
            // Port number is not a number
            return ModuleProcessExitCodes.INVALID_ARGS;
        }

        // A module process will connect to a given port number first. We try connecting to the server for 30 seconds, then give up, returning an error code in the process.
        VrcftSandboxClient client = new VrcftSandboxClient(serverPortNumber);
        client.Connect();



        return ModuleProcessExitCodes.OK;
    }
}
