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
        var buffer = new byte[4096];

        while (_loop)
        {
            try
            {
                if (_receiver.IsBound)
                {
                    var length = _receiver.Receive(buffer);
                    var messageIndex = 0;
                    
                    OscMessage msg;
                    try
                    {
                        msg = new OscMessage(buffer, length, ref messageIndex);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (msg.Value is not float) continue;

                    if (msg.Address == "/mouthFunnel" || msg.Address == "/mouthPucker")
                    {
                        BabbleExpressions.BabbleExpressionMap.SetByKey2(msg.Address, (float)msg.Value * 4);
                    }
                    else if (BabbleExpressions.BabbleExpressionMap.ContainsKey2(msg.Address))
                    {
                        BabbleExpressions.BabbleExpressionMap.SetByKey2(msg.Address, (float)msg.Value);
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