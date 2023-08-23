using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC;

namespace VRCFaceTracking.Babble;
public partial class BabbleOSC
{
    private Socket _receiver;
    private bool _loop = true;
    private readonly Thread _thread;
    private readonly ILogger _logger;
    private readonly int _resolvedPort;
    private const int DEFAULT_PORT = 8888;
    private const int TIMEOUT_MS = 10_000;

    public BabbleOSC(ILogger iLogger, int? port = null)
    {
        _logger = iLogger;

        if (_receiver != null)
        {
            _logger.LogError("BabbleOSC connection already exists.");
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
        var offset = 0;
        var buffer = new byte[4096];

        while (_loop)
        {
            try
            {
                if (_receiver.IsBound)
                {
                    var length = _receiver.Receive(buffer);
                    var oscMessage = OscMessage.TryParseOsc(buffer, length, ref offset);
                    if (oscMessage == null) continue;
                    if (oscMessage._meta.ValueLength <= 0 || oscMessage.Value is not OscValueType.Float) continue;

                    // if (string.IsNullOrEmpty(oscMessage.Address)) continue; // Should NEVER be null...
                    if (BabbleExpressionMap.ContainsKey2(oscMessage.Address))
                    {
                        BabbleExpressionMap.SetByKey2(oscMessage.Address, (float) oscMessage.Value);
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