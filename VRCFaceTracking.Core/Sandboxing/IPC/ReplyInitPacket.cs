using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class ReplyInitPacket : IpcPacket
{
    public bool eyeSuccess                  = false;
    public bool expressionSuccess           = false;
    public string ModuleInformationName     = "";

    public override PacketType GetPacketType() => PacketType.ReplyInit;

    public override byte[] GetBytes()
    {
        // Build init packet
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        int packedData  = eyeSuccess ? 1 : 0;
        packedData = packedData | ( expressionSuccess ? 2 : 0 );

        byte[] moduleInfoStringData = Encoding.UTF8.GetBytes(ModuleInformationName);
        byte[] moduleInfoSizeBytes  = BitConverter.GetBytes(moduleInfoStringData.Length);

        byte packedDataByte = (byte) packedData;

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE + 1 + sizeof(int) + moduleInfoStringData.Length;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, finalDataStream, 0, SIZE_PACKET_MAGIC);                    // Magic
        Buffer.BlockCopy(packetTypeBytes, 0, finalDataStream, 4, SIZE_PACKET_TYPE);                     // Packet Type
        finalDataStream[8] = packedDataByte;                                                            // packedDataByte
        Buffer.BlockCopy(moduleInfoSizeBytes,   0, finalDataStream, 9,  sizeof(int));                   // Length
        Buffer.BlockCopy(moduleInfoStringData,  0, finalDataStream, 13, moduleInfoStringData.Length);   // Data

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
        // Unpack data into booleans
        byte packedData = data[8];
        this.eyeSuccess = ( packedData & 1 ) == 1;
        this.expressionSuccess = ( packedData & 2 ) == 2;

        int moduleInfoStringLength  = BitConverter.ToInt32(data, 9);
        ModuleInformationName       = Encoding.UTF8.GetString(data, 13, moduleInfoStringLength);
    }
}
