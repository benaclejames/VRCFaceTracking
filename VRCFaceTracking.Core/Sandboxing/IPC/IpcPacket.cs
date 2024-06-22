namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class IpcPacket
{
    internal static readonly byte[] HANDSHAKE_MAGIC = { 0xAF, 0xEC, 0x00, 0x8D, };
    internal static int SIZE_PACKET_MAGIC => HANDSHAKE_MAGIC.Length;
    internal static int SIZE_PACKET_TYPE => 4;
    public enum PacketType : uint
    {
        // Core
        Unknown             = 0,
        Handshake           = 1,

        // Data update events
        MetadataUpdate      = 100,
        LipUpdate           = 101,
        EyeUpdate           = 102,

        // Events which invoke the actual module functions
        EventGetSupported   = 200,
        EventInit           = 201,
        EventTeardown       = 202,
        EventUpdate         = 203,
        EventLog            = 204,

        // Replies to the above events
        ReplyGetSupported   = 300,
        ReplyInit           = 301,
        
        // Debug streams are handled uniquely due to their nature
        DebugStreamFrame    = 1000,
    }

    public virtual PacketType GetPacketType() => PacketType.Unknown;

    public virtual byte[] GetBytes()
    {
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE;

        // Prepare buffer
        byte[] data = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, data, 0, SIZE_PACKET_MAGIC);
        Buffer.BlockCopy(packetTypeBytes, 4, data, 0, SIZE_PACKET_TYPE);

        return data;
    }
    public virtual void Decode(in byte[] data)
    {
    }
}
