namespace VRCFaceTracking.ModuleProcess;

public class ModuleProcessMain
{
    public static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            return ModuleProcessExitCodes.INVALID_ARGS;
        }

        string namedPipeDestination = string.Join(' ', args);

        // A module process will connect to a given named pipe first. We try connecting to the named pipe for 30 seconds, then give up, returning an error code in the process.

        return ModuleProcessExitCodes.OK;
    }
}
