using System.Reflection;
using Microsoft.Extensions.Logging;
using Valve.VR;

namespace VRCFaceTracking.Services;

public class OpenVRService
{
    private CVRSystem _system;
    private ILogger _logger;
    
    public OpenVRService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("OpenVRService");
        
        EVRInitError error = EVRInitError.None;
        _system = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
        
        if (error != EVRInitError.None)
        {
            _logger.LogWarning("Failed to initialize OpenVR: {0}", error);
            return;
        }
        
        // Our app.vrmanifest is next to the executable, so we can just use the current directory of the executable
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var fullManifestPath = Path.Combine(currentDirectory, "app.vrmanifest"); // Replace is for Linux
        var manifestRegisterResult = OpenVR.Applications.AddApplicationManifest(fullManifestPath, false);
        if (manifestRegisterResult != EVRApplicationError.None)
        {
            _logger.LogWarning("Failed to register manifest: {0}", manifestRegisterResult);
            return;
        }
        
        IsInitialized = true;
        _logger.LogInformation("Successfully initialized OpenVR");
    }
    
    public bool IsInitialized { get; }

    public bool AutoStart
    {
        get => IsInitialized && OpenVR.Applications.GetApplicationAutoLaunch("benaclejames.vrcft");
        set
        {
            if (!IsInitialized)
                return; 
            var setAutoLaunchResult = OpenVR.Applications.SetApplicationAutoLaunch("benaclejames.vrcft", value);
            if (setAutoLaunchResult != EVRApplicationError.None)
                _logger.LogError("Failed to set auto launch: {0}", setAutoLaunchResult);
        }
    }
} 