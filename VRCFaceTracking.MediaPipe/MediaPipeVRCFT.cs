using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Params.Expressions;

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

    public override void Update()
    {
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight = 
            _mp.Expressions["/tongueOut"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight = 
            _mp.Expressions["/jawOpen"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight = 
            _mp.Expressions["/jawLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight = 
            _mp.Expressions["/jawRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight = 
            _mp.Expressions["/jawForward"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight = 
            _mp.Expressions["/noseSneerLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight = 
            _mp.Expressions["/noseSneerRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthClosed].Weight = 
            _mp.Expressions["/mouthClose"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight = 
            _mp.Expressions["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight = 
            _mp.Expressions["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight = 
            _mp.Expressions["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight = 
            _mp.Expressions["/mouthPucker"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight = 
            _mp.Expressions["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight = 
            _mp.Expressions["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight = 
            _mp.Expressions["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight = 
            _mp.Expressions["/mouthFunnel"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = 
            _mp.Expressions["/mouthUpperUpLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight = 
            _mp.Expressions["/mouthUpperUpRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight = 
            _mp.Expressions["/mouthLowerDownLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight = 
            _mp.Expressions["/mouthLowerDownRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight = 
            _mp.Expressions["/mouthPressLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight = 
            _mp.Expressions["/mouthPressRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight = 
            _mp.Expressions["/mouthStretchLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight = 
            _mp.Expressions["/mouthStretchRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight = 
            _mp.Expressions["/mouthDimpleLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight = 
            _mp.Expressions["/mouthDimpleRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight = 
            _mp.Expressions["/mouthSmileLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight = 
            _mp.Expressions["/mouthSmileRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight = 
            _mp.Expressions["/mouthFrownLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight = 
            _mp.Expressions["/mouthFrownRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight = 
            _mp.Expressions["/cheekPuff"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight = 
            _mp.Expressions["/cheekPuff"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight = 
            _mp.Expressions["/browInnerUp"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight = 
            _mp.Expressions["/browInnerUp"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight = 
            _mp.Expressions["/browDownLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight = 
            _mp.Expressions["/browDownRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight = 
            _mp.Expressions["/browOuterUpLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight = 
            _mp.Expressions["/browOuterUpRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight = 
            _mp.Expressions["/eyeSquintLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight = 
            _mp.Expressions["/eyeSquintRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight = 
            -_mp.Expressions["/eyeWideLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight = 
            _mp.Expressions["/eyeWideRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight =
            _mp.Expressions["/cheekSquintLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight = 
            _mp.Expressions["/cheekSquintRight"];

        UnifiedTracking.Data.Eye.Left.Openness = 
            1 - _mp.Expressions["/eyeBlinkLeft"];
        UnifiedTracking.Data.Eye.Right.Openness = 
            1 - _mp.Expressions["/eyeBlinkRight"];
        UnifiedTracking.Data.Eye.Left.Gaze.x = 
            -_mp.Expressions["/eyeLookOutLeft"] + _mp.Expressions["/eyeLookInLeft"];
        UnifiedTracking.Data.Eye.Left.Gaze.y = 
            -_mp.Expressions["/eyeLookDownLeft"] + _mp.Expressions["/eyeLookUpLeft"];
        UnifiedTracking.Data.Eye.Right.Gaze.x = 
            -_mp.Expressions["/eyeLookInRight"] + _mp.Expressions["/eyeLookOutRight"];
        UnifiedTracking.Data.Eye.Right.Gaze.y = 
            -_mp.Expressions["/eyeLookDownRight"] + _mp.Expressions["/eyeLookUpRight"];
    }

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
