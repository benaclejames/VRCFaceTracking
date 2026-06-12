using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Valve.VR;

namespace VRCFaceTracking.Services;

public class OpenVRService(ILogger<OpenVRService> logger)
{
    private CVRSystem? _system;

    public bool Initialize()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return false;
        }

        try
        {
            EVRInitError error = EVRInitError.None;
            _system = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);

            if (error != EVRInitError.None)
            {
                logger.LogWarning("Failed to initialize OpenVR: {0}", error);
                IsInitialized = false;
                return false;
            }

            var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (currentDirectory == null)
            {
                IsInitialized = false;
                return false;
            }

            var fullManifestPath = Path.Combine(currentDirectory, "app.vrmanifest");
            var manifestRegisterResult = OpenVR.Applications.AddApplicationManifest(fullManifestPath, false);
            if (manifestRegisterResult != EVRApplicationError.None)
            {
                logger.LogWarning("Failed to register manifest: {0}", manifestRegisterResult);
                IsInitialized = false;
                return false;
            }

            logger.LogInformation("Successfully initialized OpenVR");
            IsInitialized = true;
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or BadImageFormatException or TypeInitializationException)
        {
            logger.LogWarning("OpenVR native library not available: {Message}", ex.Message);
            IsInitialized = false;
            return false;
        }
    }

    public void InitIfNotAlready()
    {
        if (!IsInitialized)
            Initialize();
    }

    public bool IsInitialized { get; private set; }

    public bool AutoStart
    {
        get
        {
            try
            {
                return IsInitialized && OpenVR.Applications.GetApplicationAutoLaunch("benaclejames.vrcft");
            }
            catch
            {
                return false;
            }
        }
        set
        {
            if (!IsInitialized && !Initialize())
            {
                logger.LogWarning("Failed to set AutoStart preference. OpenVR couldn't be initialized.");
                return;
            }

            try
            {
                var result = OpenVR.Applications.SetApplicationAutoLaunch("benaclejames.vrcft", value);
                if (result != EVRApplicationError.None)
                    logger.LogError("Failed to set auto launch: {0}", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception setting auto launch");
            }
        }
    }
}
