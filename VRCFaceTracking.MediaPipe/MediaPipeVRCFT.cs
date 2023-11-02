using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VRCFaceTracking.MediaPipe;

public class MediaPipeVRCFT : ExtTrackingModule
{
    private MediaPipeOSC _mp;
    private Process _p;
    private const string _name = "mediaPipeFaceTracking.exe";

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize
        (bool eyeAvailable, bool expressionAvailable)
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(assemblyPath, "mediapipe_config.json");

        MediaPipeConfig config;
        if (File.Exists(configPath))
        {
            Logger.LogInformation("Configuration detected.");
            Logger.LogInformation("Loading configuration from " + configPath);
            var contents = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<MediaPipeConfig>(contents)!;
        }
        else
        {
            Logger.LogInformation("No configuration detected. Creating with default settings.");
            Logger.LogInformation("Saving configuration to " + configPath);
            config = new MediaPipeConfig(); // Defaults to port 8888
            File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
        }

        // Don't start a new process if it's already running
        if (Process.GetProcessesByName(_name).Length == 0)
        {
            _p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = assemblyPath,
                    FileName = Path.Combine(assemblyPath, _name),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };

            _p.OutputDataReceived += (sender, args) => Logger.LogInformation(args.Data);
            _p.ErrorDataReceived += (sender, args) => Logger.LogError(args.Data);
            _p.Start();
        }

        List<Stream> images = new();
        {
            Assembly.
                GetExecutingAssembly().
                GetManifestResourceStream("VRCFaceTracking.MediaPipe.MediaPipeLogo.png");
        }

        ModuleInformation = new ModuleMetadata()
        {
            Name = "Web Camera Face Tracking (MediaPipe)",
            StaticImages = images
        };

        Logger.LogInformation("Using port: " + config.MediaPipeOscPort);
        _mp = new MediaPipeOSC(Logger, config.MediaPipeOscPort);

        return (true, true);
    }

    public override void Update() => _mp.Update(ref UnifiedTracking.Data);

    public override void Teardown()
    {
        if (_p != null)
        {
            _p.Close();
            _p.Dispose();
        }

        _mp.Teardown();
    }
}
