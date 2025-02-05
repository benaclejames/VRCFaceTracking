using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class ReplyTeardownPacket : IpcPacket
{
    public override PacketType GetPacketType() => PacketType.ReplyTeardown;

    // We send a challenge to the vrcft host, and if we receive a reply with the same data, we consider the connection successfully ACKed.
    // In other words, this packet is the handshake begin and ACK packet.
    public override byte[] GetBytes()
    {
        // Build handshake packet

        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, finalDataStream, 0, SIZE_PACKET_MAGIC);          // Magic
        Buffer.BlockCopy(packetTypeBytes, 0, finalDataStream, 4, SIZE_PACKET_TYPE);           // Packet Type

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {

    }

}
