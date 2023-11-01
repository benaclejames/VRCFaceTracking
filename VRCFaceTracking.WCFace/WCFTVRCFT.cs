using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace VRCFaceTracking.WCFace;

public class WCFTVRCFT : ExtTrackingModule
{
    private WCFTData lastWCFTData = new();
    private Process process;
    private Thread thread;
    private WebSocket cws;
    private CancellationTokenSource worker_ct = new();
    private const string URIString = "ws://localhost:7010/";
    private string lastData = string.Empty;
    private bool didLoad;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public void VerifyDeadThread()
    {
        if (thread != null)
        {
            if (thread.IsAlive)
                thread.Join();
        }
        worker_ct = new CancellationTokenSource();
        thread = null;
    }

    public void VerifyClosedSocket()
    {
        if (cws != null)
        {
            if (cws.ReadyState == WebSocketState.Open)
                cws.Close();
        }
        cws = null;
    }

    public override (bool eyeSuccess, bool expressionSuccess) Initialize
        (bool eyeAvailable, bool expressionAvailable)
    {
        foreach (var process in Process.GetProcessesByName("facetrackerNeos"))
        {
            process.Kill();
        }

        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var configPath = Path.Combine(assemblyPath, "wcft_config.json");

        WCFTConfig config;
        if (!File.Exists(configPath))
        {
            WCFTConfig c = new WCFTConfig();
            File.WriteAllText(configPath, JsonConvert.SerializeObject(c));
            config = c;
            Logger.LogInformation("No config detected. Creating with default settings.");
            Logger.LogInformation("Saving config to " + configPath);
        }
        else
        {
            var contents = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<WCFTConfig>(contents)!;
            Logger.LogInformation("Config detected.");
            Logger.LogInformation("Loading config from " + configPath);
        }

        Logger.LogInformation("Camera Number: " + config.cameraNum);
        Logger.LogInformation("Resolution Index: " + config.resolution);
        Logger.LogInformation("Model Index: " + config.model);
        Logger.LogInformation("Smooth Translation (0 no, 1 yes): " + config.smootht);
        Logger.LogInformation("Smooth Rotation (0 no, 1 yes): " + config.smoothr);

        var args = string.Format(
            "-c {0} -D {1} --model {2} --smooth-translation {3} --smooth-rotation {4}", 
            config.cameraNum, config.resolution, config.model, config.smootht, config.smoothr);

        var path = Path.Combine(assemblyPath, "WCFT", "Binary");
        process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                WorkingDirectory = path,
                FileName = Path.Combine(path, "facetrackerNeos.exe"),
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.OutputDataReceived += (sender, args) => Logger.LogInformation(args.Data);
        process.ErrorDataReceived += (sender, args) => Logger.LogError(args.Data);
        process.Start();

        List<Stream> streams = new List<Stream>();
        Assembly a = Assembly.GetExecutingAssembly();
        var hmdStream = a.GetManifestResourceStream
            ("VRCFaceTracking.WCFace.Webcamera.png");
        streams.Add(hmdStream!);
        ModuleInformation = new ModuleMetadata()
        {
            Name = "Webcamera Face Tracking (WCFace)",
            StaticImages = streams
        };

        VerifyClosedSocket();
        cws = new WebSocket(URIString);
        cws.Connect();
        var isConnected = cws.ReadyState == WebSocketState.Open;
        didLoad = isConnected;
        VerifyClosedSocket();
        StartThread();
        return (true, true);
    }

    public void StartThread()
    {
        VerifyDeadThread();
        thread = new Thread(() =>
        {
            // Start the Socket
            VerifyClosedSocket();
            cws = new WebSocket(URIString);
            cws.OnMessage += (sender, args) => lastData = args.Data.ToString();
            cws.Connect();
            Thread.Sleep(2500);
            if (cws.ReadyState == WebSocketState.Open)
            {
                // Start the loop
                var isLoading = false;
                while (!worker_ct.IsCancellationRequested)
                {
                    if (cws.ReadyState == WebSocketState.Open)
                    {
                        isLoading = false;
                        // Send a Ping Message
                        cws.Send("");
                        GetWebsocketData();
                    }
                    else
                    {
                        if (didLoad && !isLoading)
                        {
                            // Socket will randomly force close because it thinks its being DOSsed
                            // We'll just re-open it if it thinks this
                            // (thanks python websockets)
                            isLoading = true;
                            cws = new WebSocket(URIString);
                            cws.OnMessage += (sender, args) => lastData = args.Data.ToString();
                            cws.Connect();
                        }
                    }
                    // Please don't change this to anything lower than 50
                    Thread.Sleep(50);
                }
            }
            VerifyClosedSocket();
        });
        thread.Start();
    }

    public void GetWebsocketData()
    {
        if (cws.ReadyState == WebSocketState.Open)
        {
            WCFTData newWCFTData = new WCFTData();
            var splitData = WCFTParser.SplitMessage(lastData);

            // Make sure it's valid data, then begin parsing!
            if (!WCFTParser.IsMessageValid(splitData))
            {
                return;
            }

            for (var i = 0; i < splitData.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        newWCFTData.IsFaceTracking = WCFTParser.GetValueFromNeosArray<bool>(splitData[i]);
                        break;
                    case 1:
                    case 2:
                        break;
                    case 3:
                        newWCFTData.LeftEyeBlink =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 0);
                        newWCFTData.RightEyeBlink =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 1);
                        break;
                    case 4:
                        newWCFTData.MouthOpen =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 0);
                        newWCFTData.MouthWide =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 1);
                        break;
                    case 5:
                        newWCFTData.LeftEyebrowUpDown =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 0);
                        newWCFTData.RightEyebrowUpDown =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 1);
                        break;
                    case 6:
                        newWCFTData.LookUpDown =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 0);
                        newWCFTData.LookLeftRight =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 1);
                        break;
                    case 7:
                        newWCFTData.EyebrowSteepness =
                            (float)WCFTParser.GetValueFromNeosArray<double>(splitData[i], 0);
                        break;
                }
            }
            lastWCFTData = newWCFTData;
        }
    }

    public override void Update()
    {
        if (!lastWCFTData.IsFaceTracking) return;

        Vector2 gaze = new Vector2(
            Math.Clamp(lastWCFTData.LookLeftRight, 0f, 1f),
            Math.Clamp(lastWCFTData.LookUpDown * -1f, 0f, 1f)
        );
        UnifiedTracking.Data.Eye.Left.Gaze = gaze;
        UnifiedTracking.Data.Eye.Right.Gaze = gaze;
        UnifiedTracking.Data.Eye.Left.Openness = Math.Clamp(lastWCFTData.LeftEyeBlink, 0f, 1f);
        UnifiedTracking.Data.Eye.Right.Openness = Math.Clamp(lastWCFTData.RightEyeBlink, 0f, 1f);

        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.JawOpen].Weight = lastWCFTData.MouthOpen;
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.MouthCornerPullLeft].Weight = lastWCFTData.MouthWide;
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.MouthCornerPullRight].Weight = lastWCFTData.MouthWide;
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.EyeWideLeft].Weight = 
            Math.Clamp(lastWCFTData.LeftEyebrowUpDown, 0f, 1f);
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.EyeWideRight].Weight = 
            Math.Clamp(lastWCFTData.RightEyebrowUpDown, 0f, 1f);
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.EyeSquintLeft].Weight =
            Math.Clamp(lastWCFTData.LeftEyebrowUpDown, -1f, 0f) * -1f;
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.EyeSquintRight].Weight =
            Math.Clamp(lastWCFTData.RightEyebrowUpDown, -1f, 0f) * -1f;
        var clamped = Math.Clamp(lastWCFTData.EyebrowSteepness, 0f, 1f);
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight = clamped;
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight = clamped;
    }

    public override void Teardown()
    {
        process.Close();
        process.Dispose();
        worker_ct.Cancel();
    }
}