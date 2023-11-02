using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.MediaPipe;

public partial class MediaPipeOSC
{
    // Expressions defined in MediaPipeExpressions.cs
    private Socket _receiver;
    private bool _loop = true;
    private readonly Thread _thread;
    private readonly ILogger _logger;
    private readonly int _resolvedPort;
    private const int DEFAULT_PORT = 8888;
    private const int TIMEOUT_MS = 10_000;
    private const string DEFAULT_IP = "127.0.0.1";

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
        _receiver.Bind(new IPEndPoint(IPAddress.Parse(DEFAULT_IP), _resolvedPort));
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
                    _receiver.Bind(new IPEndPoint(IPAddress.Parse(DEFAULT_IP), _resolvedPort));
                    _receiver.ReceiveTimeout = TIMEOUT_MS;
                    continue;
                }

                var length = _receiver.Receive(buffer);
                var offset = 0;
                var oscMessage = new OscMessage(buffer, length, ref offset);
                if (oscMessage == null || oscMessage._meta.ValueLength <= 0) continue;

                if (Expressions.ContainsKey1(oscMessage.Address))
                {
                    Expressions.SetByKey1(oscMessage.Address, (float)oscMessage.Value);
                }
            }
            catch (Exception)  { }
        }
    }

    public void Update(ref UnifiedTrackingData data)
    {
        // Map known expressions
        foreach (var address in Expressions.OuterKeys)
        {
            var weight = Expressions.GetByKey1(address);
            foreach (var exp in Expressions.GetInnerKey(address))
            {
                data.Shapes[(int)exp].Weight = weight;
            }
        }

        // Map eye/unknown expressions
        UnifiedTracking.Data.Eye.Left.Openness =
            1f - Expressions.GetByKey1("/eyeBlinkLeft");
        UnifiedTracking.Data.Eye.Right.Openness =
            1f - Expressions.GetByKey1("/eyeBlinkRight");
        UnifiedTracking.Data.Eye.Left.Gaze = new Vector2(
            -Expressions.GetByKey1("/eyeLookOutLeft") + Expressions.GetByKey1("/eyeLookInLeft"),
            -Expressions.GetByKey1("/eyeLookDownLeft") + Expressions.GetByKey1("/eyeLookUpLeft"));
        UnifiedTracking.Data.Eye.Right.Gaze = new Vector2(
            -Expressions.GetByKey1("/eyeLookInRight") + Expressions.GetByKey1("/eyeLookOutRight"),
            -Expressions.GetByKey1("/eyeLookDownRight") + Expressions.GetByKey1("/eyeLookUpRight"));
    }

    public void Teardown()
    {
        _loop = false;
        _receiver.Close();
        _receiver.Dispose();
        _thread.Join();
    }
}