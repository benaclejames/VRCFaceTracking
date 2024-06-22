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
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Sandboxing;

public delegate void OnPacketReceivedCallback(in IpcPacket packet);

public class VrcftSandboxClient : UdpFullDuplex
{
    // private int                                     _port = 0;
    private IPEndPoint                              _serverEndpoint;
    private readonly ILoggerFactory                 _loggerFactory;
    private readonly ILogger<VrcftSandboxClient>    _logger;
    private bool                                    _isConnected;

    public OnPacketReceivedCallback OnPacketReceivedCallback = null;
    public VrcftSandboxClient(int portNumber,
        ILoggerFactory factory
        ) : base(0, null, new IPEndPoint(IPAddress.Loopback, portNumber)) // 0 is reserved for the OS to pick for us
    {
        _isConnected = false;
        // Init loggers
        _loggerFactory = factory;
        _logger = factory.CreateLogger<VrcftSandboxClient>();

        var addresses = Dns.GetHostAddresses("127.0.0.1");
        if ( addresses.Length == 0 )
        {
            throw new Exception($"Unable to find localhost (how did this even happen??)");
        }

        _serverEndpoint = new IPEndPoint(addresses[0], portNumber);
        Port = ( ( IPEndPoint )_receivingUdpClient.Client.LocalEndPoint ).Port;
        
        _logger.LogInformation($"Starting sandbox process on port {Port}...");
    }

    public void Connect()
    {
        var handshakePkt = new HandshakePacket();
        _logger.LogInformation($"Attempting to connect to server...");
        SendData(handshakePkt, _serverEndpoint);
    }

    public override void OnBytesReceived(in byte[] data, in IPEndPoint endpoint)
    {
        bool decodeResult = VrcftPacketDecoder.TryDecodePacket(data, out IpcPacket packet);

        if ( decodeResult )
        {
            // Tell the callback that we've received a packet
            if ( OnPacketReceivedCallback != null && packet.GetPacketType() != IpcPacket.PacketType.Unknown )
            {
                OnPacketReceivedCallback(packet);
            }

            if ( packet.GetPacketType() == IpcPacket.PacketType.Handshake )
            {
                // Handshake request
                var handshakePacket = (HandshakePacket) packet;
                if ( handshakePacket.IsValid )
                {
                    _logger.LogInformation($"Received ACK from host on port {endpoint.Port}. Handshake done.");
                    _isConnected = true;
                    SendAllPendingPackets();
                }
            }
        }
    }

    private void SendData(in byte[] message)
    {
        _receivingUdpClient.Send(message, message.Length, _serverEndpoint);
    }
    public void SendData(in IpcPacket packet)
    {
        if ( _isConnected || packet.GetPacketType() == IpcPacket.PacketType.Handshake)
        {
            SendData(packet.GetBytes());
        }
        else
        {
            _eventBus.Push(packet);
        }
    }

    public void SendAllPendingPackets()
    {
        if ( _isConnected )
        {
            if ( _eventBus.Count > 0 )
            {
                while ( _eventBus.Count > 0 )
                {
                    IpcPacket pkt = _eventBus.Pop<IpcPacket>();
                    SendData(pkt);
                }
            }
        }
    }
}