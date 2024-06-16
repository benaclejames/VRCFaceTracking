using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Sandboxing;
public class VrcftSandboxClient
{
    private UdpClient? _udpClient = null;
    private IAsyncResult _receiveAsyncResult = null;
    private IPAddress? _mainProcessIp = null;
    private IPEndPoint? _mainProcessEndpoint = null;
    private int _desiredPort = 0;
    private bool _isConnectionOk = false;

    public VrcftSandboxClient(int portNumber)
    {
        _desiredPort = portNumber;

        _mainProcessIp = IPAddress.Parse("127.0.0.1");
        _mainProcessEndpoint = new IPEndPoint(_mainProcessIp, _desiredPort); // Port 0 is reserved as assign to anything
        _udpClient = new UdpClient(_mainProcessEndpoint);
    }

    public bool IsConnected => _udpClient != null && _isConnectionOk;

    public void Connect()
    {
        if ( !IsConnected )
        {
            // Create handshake packet
            _udpClient?.Connect(_mainProcessIp, _desiredPort);
            _isConnectionOk = VerifyConnection();
        }
    }

    private void SendData(ref byte[] data)
    {
        _udpClient?.Send(data, data.Length);
    }

    internal bool VerifyConnection()
    {
        // Do handshake, and verify versions
        var handshakePkt = new HandshakePacket();

        var data = handshakePkt.GetBytes();
        SendData(ref data);

        StartReceive();

        return false;
    }

    private void StartReceive()
    {
        if ( _udpClient == null )
        {
            throw new WebException("UDP Client cannot be null.");
        }
        _receiveAsyncResult = _udpClient.BeginReceive(ReceiveHandler, new object());
    }

    private void ReceiveHandler(IAsyncResult asyncResult)
    {
        if ( _udpClient == null )
        {
            throw new WebException("UDP Client cannot be null.");
        }
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, _desiredPort);
        byte[] bytes = _udpClient.EndReceive(_receiveAsyncResult, ref ip);

        bool decodeResult = VrcftPacketDecoder.TryDecodePacket(ref bytes, out IpcPacket packet);

        // @TODO: Use packet

        StartReceive();
    }
}
