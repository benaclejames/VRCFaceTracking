using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class HandshakePacket : IpcPacket
{

    /*
     
    HEADER:   ------------
    MAGIC     AF EC 00 8D 
    PKT_ID    
    BODY:     ------------
    CHALLENGE ?? ?? ?? ??
    PID       00 00 00 00

     */

    private static readonly byte[] HANDSHAKE_CHALLENGE = new byte[] { 0xDE, 0xC1, 0xCA, 0x12, 0XD0 };
    private bool _isValid = true;
    private int _pid = 0;
    public bool IsValid => _isValid;
    public int PID => _pid;

    public override PacketType GetPacketType() => PacketType.Handshake;

    public HandshakePacket()
    {
        _pid = Process.GetCurrentProcess().Id;
    }

    // We send a challenge to the vrcft host, and if we receive a reply with the same data, we consider the connection successfully ACKed.
    // In other words, this packet is the handshake begin and ACK packet.
    public override byte[] GetBytes()
    {
        // Build handshake packet

        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());
        byte[] pidBytes = BitConverter.GetBytes(_pid);

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE + HANDSHAKE_CHALLENGE.Length + sizeof(int);

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC,       0, finalDataStream, 0, SIZE_PACKET_MAGIC);          // Magic
        Buffer.BlockCopy(packetTypeBytes,       0, finalDataStream, 4, SIZE_PACKET_TYPE);           // Packet Type
        Buffer.BlockCopy(HANDSHAKE_CHALLENGE,   0, finalDataStream, 8, HANDSHAKE_CHALLENGE.Length); // Handshake Challenge
        Buffer.BlockCopy(pidBytes,              0, finalDataStream, 13, pidBytes.Length);           // PID

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
        // Verify handshake challenge
        for ( int i = 0; i < HANDSHAKE_CHALLENGE.Length; i++ )
        {
            if ( data[i + 8] != HANDSHAKE_CHALLENGE[i] )
            {
                _isValid = false;
                break;
            }
        }
        _pid = BitConverter.ToInt32(data, 13);
        _isValid = true;
    }
}