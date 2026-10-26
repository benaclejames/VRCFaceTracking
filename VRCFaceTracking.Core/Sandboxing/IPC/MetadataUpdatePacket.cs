namespace VRCFaceTracking.Core.Sandboxing.IPC;

/// <summary>
/// Sent by a module process to ask VRCFaceTracking to reinitialize every loaded module, e.g. after the module's
/// tracking capabilities changed at runtime. Carries no payload.
/// </summary>
public class MetadataUpdatePacket : IpcPacket
{
    public override PacketType GetPacketType() => PacketType.MetadataUpdate;

    public override byte[] GetBytes()
    {
        var packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        var packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE;

        // Prepare buffer
        var finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, finalDataStream, 0, SIZE_PACKET_MAGIC);          // Magic
        Buffer.BlockCopy(packetTypeBytes, 0, finalDataStream, 4, SIZE_PACKET_TYPE);           // Packet Type

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
    }
}
