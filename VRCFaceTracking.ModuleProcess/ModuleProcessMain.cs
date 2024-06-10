using VRCFaceTracking.Core.Models;

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

        string namedPipeDestination = string.Join(' ', args);

        // A module process will connect to a given named pipe first. We try connecting to the named pipe for 30 seconds, then give up, returning an error code in the process.
        VrcftSandboxClient client = new VrcftSandboxClient(namedPipeDestination);
        client.Connect();

        SubprocessData subprocessData = new SubprocessData();
        UnifiedTrackingProxy.UpdateSharedData(ref subprocessData);

        return ModuleProcessExitCodes.OK;
    }
}
