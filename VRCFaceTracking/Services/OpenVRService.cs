using System.Reflection;
using Microsoft.Extensions.Logging;
using Valve.VR;

namespace VRCFaceTracking.Services;

public class OpenVRService
{
    private CVRSystem _system;
    private readonly ILogger<OpenVRService> _logger;
    
    public OpenVRService(ILogger<OpenVRService> logger)
    {
        _logger = logger;
    }

    public bool Initialize()
    {
        EVRInitError error = EVRInitError.None;
        _system = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
        
        if (error != EVRInitError.None)
        {
            _logger.LogWarning("Failed to initialize OpenVR: {0}", error);
            IsInitialized = false;
            return IsInitialized;
        }
        
        // Our app.vrmanifest is next to the executable, so we can just use the current directory of the executable
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var fullManifestPath = Path.Combine(currentDirectory, "app.vrmanifest"); // Replace is for Linux
        var manifestRegisterResult = OpenVR.Applications.AddApplicationManifest(fullManifestPath, false);
        if (manifestRegisterResult != EVRApplicationError.None)
        {
            _logger.LogWarning("Failed to register manifest: {0}", manifestRegisterResult);
            IsInitialized = false;
            return IsInitialized;
        }
        
        _logger.LogInformation("Successfully initialized OpenVR");
        
        IsInitialized = true;
        return IsInitialized;
    }

    public void InitIfNotAlready()
    {
        if (!IsInitialized)
        {
            Initialize();
        }
    }
    
    public bool IsInitialized { get; private set; }

    public bool AutoStart
    {
        get => IsInitialized && OpenVR.Applications.GetApplicationAutoLaunch("benaclejames.vrcft");
        set
        {
            if (!IsInitialized && !Initialize())
            {
                _logger.LogWarning("Failed to set AutoStart preference. OpenVR couldn't be initialized.");
                return;
            }

            var setAutoLaunchResult = OpenVR.Applications.SetApplicationAutoLaunch("benaclejames.vrcft", value);
            if (setAutoLaunchResult != EVRApplicationError.None)
            {
                _logger.LogError("Failed to set auto launch: {0}", setAutoLaunchResult);
            }
        }
    }
} 