using System.Reflection;
using Microsoft.Extensions.Logging;
using Valve.VR;
using Windows.ApplicationModel.Resources;

namespace VRCFaceTracking.Services;

public class OpenVRService
{
    private CVRSystem _system;
    private ILogger _logger;
    private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Resources");

    public OpenVRService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(_loader.GetString("OpenVRService"));
        
        EVRInitError error = EVRInitError.None;
        _system = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);
        
        if (error != EVRInitError.None)
        {
            _logger.LogWarning(_loader.GetString("FailedInitializeOpenVR"), error);
            return;
        }
        
        // Our app.vrmanifest is next to the executable, so we can just use the current directory of the executable
        var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var fullManifestPath = Path.Combine(currentDirectory, "app.vrmanifest"); // Replace is for Linux
        var manifestRegisterResult = OpenVR.Applications.AddApplicationManifest(fullManifestPath, false);
        if (manifestRegisterResult != EVRApplicationError.None)
        {
            _logger.LogWarning(_loader.GetString("FailedRegisterManifest"), manifestRegisterResult);
            return;
        }
        
        IsInitialized = true;
        _logger.LogInformation(_loader.GetString("SuccessfullyInitializedOpenVR"));
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
                _logger.LogError(_loader.GetString("FailedSetAutoLaunch"), setAutoLaunchResult);
        }
    }
} 