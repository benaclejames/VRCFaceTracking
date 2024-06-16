namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class IpcPacket
{
    internal static readonly byte[] HANDSHAKE_MAGIC = { 0xAF, 0xEC, 0x00, 0x8D, };
    internal static int SIZE_PACKET_MAGIC => HANDSHAKE_MAGIC.Length;
    internal static int SIZE_PACKET_TYPE => 4;
    internal enum PacketType : uint
    {
        // Core
        Unknown = 0,
        Handshake,

        // Data update events
        MetadataUpdate,
        LipUpdate,
        EyeUpdate,

        // Events which invoke the actual module functions
        EventInit,
        EventTeardown,
        EventUpdate,
        EventLog,
        
        // Debug streams are handled uniquely due to their nature
        DebugStreamFrame,
    }

    public virtual byte[] GetBytes()
    {
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)PacketType.Unknown);

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE;

        // Prepare buffer
        byte[] data = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, data, 0, SIZE_PACKET_MAGIC);
        Buffer.BlockCopy(packetTypeBytes, 4, data, 0, SIZE_PACKET_TYPE);

        return data;
    }
    public virtual void Decode(ref byte[] data)
    {
    }
}
