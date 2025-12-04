using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Library;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class EventStatusUpdatePacket : IpcPacket
{
    public ModuleState ModuleState;

    public override PacketType GetPacketType() => PacketType.EventUpdateStatus;

    public override byte[] GetBytes()
    {
        // Build init packet
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());
        byte[] moduleStatePacket = BitConverter.GetBytes((int)ModuleState);

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE + moduleStatePacket.Length;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC,   0, finalDataStream, 0, SIZE_PACKET_MAGIC);          // Magic
        Buffer.BlockCopy(packetTypeBytes,   0, finalDataStream, 4, SIZE_PACKET_TYPE);           // Packet Type
        Buffer.BlockCopy(moduleStatePacket, 0, finalDataStream, 8, moduleStatePacket.Length);   // Module State

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
        ModuleState = (ModuleState) BitConverter.ToInt32(data, 8);
    }
}
