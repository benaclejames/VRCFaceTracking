using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.ETVR;

public class ETVR_OSC
{
    public readonly Dictionary<string, float> EyeDataWithAddress = new() {
        { "/avatar/parameters/LeftEyeX", 0f },
        { "/avatar/parameters/RightEyeX", 0f },
        { "/avatar/parameters/EyesY", 0f },
        { "/avatar/parameters/LeftEyeLidExpandedSqueeze", 0f },
        { "/avatar/parameters/RightEyeLidExpandedSqueeze", 0f },
        { "/avatar/parameters/EyesDilation", 0f },
    };

    private Socket _receiver;
    private bool _loop = true;
    private readonly Thread _thread;
    private readonly ILogger _logger;
    private readonly int _resolvedPort;
    private const int DEFAULT_PORT = 8888;
    private const int TIMEOUT_MS = 10_000;

    public ETVR_OSC(ILogger iLogger, int? port = null)
    {
        _logger = iLogger;

        if (_receiver != null)
        {
            _logger.LogError("ETVR connection already exists.");
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
                if (_receiver.IsBound)
                {
                    var length = _receiver.Receive(buffer);
                    var offset = 0;
                    var oscMessage = new OscMessage(buffer, length, ref offset);
                    if (oscMessage == null) continue;
                    if (oscMessage._meta.ValueLength <= 0) continue;

                    if (EyeDataWithAddress.ContainsKey(oscMessage.Address))
                    {
                        EyeDataWithAddress[oscMessage.Address] = (float) oscMessage.Value;
                    }
                }
                else
                {
                    _receiver.Close();
                    _receiver.Dispose();
                    _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _receiver.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _resolvedPort));
                    _receiver.ReceiveTimeout = TIMEOUT_MS;
                }
            }
            catch (Exception) { }
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