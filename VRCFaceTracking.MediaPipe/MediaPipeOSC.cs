using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.MediaPipe;

public class MediaPipeOSC
{
    public readonly Dictionary<string, float> Expressions = new()
    {
        { "/_neutral", 0f }, //Unused
        { "/browDownLeft", 0f },
        { "/browDownRight", 0f },
        { "/browInnerUp", 0f },
        { "/browOuterUpLeft", 0f},
        { "/browOuterUpRight", 0f },
        { "/cheekPuff", 0f },
        { "/cheekSquintLeft", 0f },
        { "/cheekSquintRight",0f },
        { "/eyeBlinkLeft", 0f },
        { "/eyeBlinkRight",0f },
        { "/eyeLookDownLeft",0f },
        { "/eyeLookDownRight",0f },
        { "/eyeLookInLeft",0f },
        { "/eyeLookInRight",0f },
        { "/eyeLookOutLeft", 0f },
        { "/eyeLookOutRight", 0f },
        { "/eyeLookUpLeft", 0f },
        { "/eyeLookUpRight", 0f },
        { "/eyeSquintLeft",0f },
        { "/eyeSquintRight", 0f },
        { "/eyeWideLeft" ,0f},
        { "/eyeWideRight", 0f },
        { "/jawForward", 0f },
        { "/jawLeft", 0f },
        { "/jawOpen", 0f},
        { "/jawRight", 0f },
        { "/mouthClose", 0f },
        { "/mouthDimpleLeft", 0f },
        { "/mouthDimpleRight", 0f},
        { "/mouthFrownLeft",0f },
        { "/mouthFrownRight", 0f },
        { "/mouthFunnel", 0f },
        { "/mouthLeft", 0f },
        { "/mouthLowerDownLeft", 0f },
        { "/mouthLowerDownRight", 0f },
        { "/mouthPressLeft", 0f },
        { "/mouthPressRight",0f },
        { "/mouthPucker", 0f } ,
        { "/mouthRight", 0f },
        { "/mouthRollLower", 0f }, //Unused
        { "/mouthRollUpper", 0f }, //Unused
        { "/mouthShrugLower", 0f },
        { "/mouthShrugUpper", 0f },
        { "/mouthSmileLeft", 0f },
        { "/mouthSmileRight", 0f },
        { "/mouthStretchLeft", 0f },
        { "/mouthStretchRight",0f },
        { "/mouthUpperUpLeft", 0f },
        { "/mouthUpperUpRight", 0f },
        { "/noseSneerLeft", 0f },
        { "/noseSneerRight", 0f },
        { "/tongueOut", 0f },
    };

    private Socket _receiver;
    private bool _loop = true;
    private readonly Thread _thread;
    private readonly ILogger _logger;
    private readonly int _resolvedPort;
    private const int DEFAULT_PORT = 8888;
    private const int TIMEOUT_MS = 10_000;

    public MediaPipeOSC(ILogger iLogger, int? port = null)
    {
        _logger = iLogger;

        if (_receiver != null)
        {
            _logger.LogError("MediaPipe OSC connection already exists.");
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
        var buffer = new byte[4096];
        
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

                if (Expressions.ContainsKey(oscMessage.Address))
                {
                    Expressions[oscMessage.Address] = (float)oscMessage.Value;
                }
            }
            catch (Exception)  { }
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