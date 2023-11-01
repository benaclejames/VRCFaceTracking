using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.VSeeFace;

public class VSFOSC
{
    // Expressions with non-direct face mappings/eye expressions
    public readonly Dictionary<string, float> VSFDataWithoutMapping = new() {
        { "A", 0f },
        { "I", 0f },
        { "U", 0f },
        { "E", 0f },
        { "O", 0f },
        { "Blink", 0f },
        { "Blink_L", 0f },
        { "Blink_R", 0f },
        { "LookUp", 0f },
        { "LookDown", 0f },
        { "LookLeft", 0f },
        { "LookRight", 0f },
        { "Neutral", 0f },
        { "OK", 0f },
    };

    // Expressions with known face mappings
    private readonly TwoKeyDictionary<string, HashSet<UnifiedExpressions>, float> VSFDataWithMapping = new() {
        { "Joy", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthCornerPullLeft,
            UnifiedExpressions.MouthCornerPullRight,
            }, 0f
        },
        { "Angry", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthFrownLeft,
            UnifiedExpressions.MouthFrownRight,
            }, 0f
        },
        { "Sorrow", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthFrownLeft,
            UnifiedExpressions.MouthFrownRight,
            }, 0f
        },
        { "Fun", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthCornerPullLeft,
            UnifiedExpressions.MouthCornerPullRight,
            }, 0f
        },
        { "Surprised", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.EyeWideLeft,
            UnifiedExpressions.EyeWideRight,
            }, 0f
        },
    };

    private Socket _receiver;
    private bool _loop = true;
    private readonly Thread _thread;
    private readonly ILogger _logger;
    private readonly int _resolvedPort;
    private const int DEFAULT_PORT = 8888;
    private const int TIMEOUT_MS = 10_000;

    public VSFOSC(ILogger iLogger, int? port = null)
    {
        _logger = iLogger;

        if (_receiver != null)
        {
            _logger.LogError("VSF OSC connection already exists.");
            return;
        }

        _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _resolvedPort = port ?? DEFAULT_PORT;
        _receiver.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _resolvedPort));
        _receiver.ReceiveTimeout = TIMEOUT_MS;

        _loop = true;
        _thread = new Thread(new ThreadStart(ListenLoop));
        _thread.Start();
    }

    private void ListenLoop()
    {
        var buffer = new byte[5556];
        
        while (_loop)
        {
            try
            {
                if (!_receiver.IsBound)
                {
                    _receiver.Close();
                    _receiver.Dispose();
                    _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _receiver.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _resolvedPort));
                    _receiver.ReceiveTimeout = TIMEOUT_MS;
                    continue;
                }

                var length = _receiver.Receive(buffer);
                var offset = 0;
                var oscMessage = new OscMessage(buffer, length, ref offset);

                if (oscMessage == null || oscMessage._meta.ValueLength <= 0) continue;
                else if (oscMessage.Address != "/VMC/Ext/Blend/Val") // Fail fast if not a blend value....
                {
                    if (oscMessage.Address == "/VMC/Ext/OK") // ...but note the OK message
                    {
                        VSFDataWithoutMapping["OK"] = (float)oscMessage.Value;
                    }
                    continue;
                }

                var exp = (string)oscMessage.Value;

                if (VSFDataWithoutMapping.ContainsKey(exp))
                {
                    VSFDataWithoutMapping[oscMessage.Address] = weight;
                }
                else if (VSFDataWithMapping.ContainsKey1(exp))
                {
                    VSFDataWithMapping.SetByKey1(oscMessage.Address, weight);
                }
                
            }
            catch (Exception) 
            { 
                _logger.LogError("Incoming packet was too large!");
                _logger.LogError("Did you disable packet bundling in VSeeFace?");
            }
        }
    }

    public void MapKnownExpressions(ref UnifiedTrackingData data)
    {
        foreach (var address in VSFDataWithMapping.OuterKeys)
        {
            var weight = VSFDataWithMapping.GetByKey1(address);
            foreach (var exp in VSFDataWithMapping.GetInnerKey(address))
            {
                data.Shapes[(int)exp].Weight = weight;
            }         
        }
    }

    public void Teardown()
    {
        _loop = false;
        _receiver.Close();
        _receiver.Dispose();
        _thread.Join();
    }
}