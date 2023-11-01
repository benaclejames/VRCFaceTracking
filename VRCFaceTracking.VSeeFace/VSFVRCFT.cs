using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.VSeeFace;

public class VSFVRCFT : ExtTrackingModule
{
    private VSFOSC _vsf;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize
        (bool eyeAvailable, bool expressionAvailable)
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(assemblyPath, "vsf_config.json");

        VSFConfig config;
        if (File.Exists(configPath))
        {
            Logger.LogInformation("Configuration detected.");
            Logger.LogInformation("Loading configuration from " + configPath);
            var contents = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<VSFConfig>(contents)!;
        }
        else
        {
            Logger.LogInformation("No configuration detected. Creating with default settings.");
            Logger.LogInformation("Saving configuration to " + configPath);
            config = new VSFConfig(); // Defaults to port 39539
            File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
        }

        List<Stream> images = new();
        {
            Assembly.
                GetExecutingAssembly().
                GetManifestResourceStream("VRCFaceTracking.VSeeFace.VSF.png");
        }

        ModuleInformation = new ModuleMetadata()
        {
            Name = "Web Camera Face Tracking (VSeeFace)",
            StaticImages = images
        };

        Logger.LogInformation("Using port: " + config.VSFOscPort);
        _vsf = new VSFOSC(Logger, config.VSFOscPort);
        
        return (true, true);
    }
    public override void Update()
    {
        if (_vsf.VSFDataWithoutMapping["OK"] != 1f) return;

        _vsf.MapKnownExpressions(ref UnifiedTracking.Data);

        Vector2 gaze = new Vector2(
            _vsf.VSFDataWithoutMapping["LookUp"] - 
            _vsf.VSFDataWithoutMapping["LookDown"],
            _vsf.VSFDataWithoutMapping["LookLeft"] - 
            _vsf.VSFDataWithoutMapping["LookRight"]
        );

        UnifiedTracking.Data.Eye.Left.Gaze = gaze;
        UnifiedTracking.Data.Eye.Right.Gaze = gaze;
        UnifiedTracking.Data.Eye.Left.Openness = 
            1f - _vsf.VSFDataWithoutMapping["Blink_L"];
        UnifiedTracking.Data.Eye.Right.Openness =
            1f - _vsf.VSFDataWithoutMapping["Blink_R"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight = 
            (_vsf.VSFDataWithoutMapping["O"] + _vsf.VSFDataWithoutMapping["A"]) / 2f;

        var e = _vsf.VSFDataWithoutMapping["E"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight = e;
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight = e;
    }

    public override void Teardown() => _vsf.Teardown();
}
