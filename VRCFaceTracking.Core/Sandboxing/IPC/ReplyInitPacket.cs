using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class ReplyInitPacket : IpcPacket
{
    public bool eyeAvailable        = false;
    public bool expressionAvailable = false;

    public override PacketType GetPacketType() => PacketType.ReplyInit;

    public override byte[] GetBytes()
    {
        // Build init packet
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE + 1;

        int packedData  = eyeAvailable ? 1 : 0;
        packedData = packedData | ( expressionAvailable ? 2 : 0 );

        byte packedDataByte = (byte) packedData;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, finalDataStream, 0, SIZE_PACKET_MAGIC);    // Magic
        Buffer.BlockCopy(packetTypeBytes, 0, finalDataStream, 4, SIZE_PACKET_TYPE);     // Handshake
        finalDataStream[8] = packedDataByte;                                            // packedDataByte

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
        // Unpack data into booleans
        byte packedData = data[8];
        this.eyeAvailable = ( packedData & 1 ) == 1;
        this.expressionAvailable = ( packedData & 2 ) == 2;
    }
}
