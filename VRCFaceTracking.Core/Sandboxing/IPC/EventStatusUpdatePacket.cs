using VRCFaceTracking.Core.Library;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class EventStatusUpdatePacket : IpcPacket
{
    public ModuleState ModuleState;
    public bool UsingEye, UsingExpression;

    public override PacketType GetPacketType() => PacketType.EventUpdateStatus;

    public override byte[] GetBytes()
    {
        // Build init packet
        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());
        byte[] moduleStatePacket = BitConverter.GetBytes((int)ModuleState);
        byte[] usingEyePacket = BitConverter.GetBytes(UsingEye);
        byte[] usingExpressionPacket = BitConverter.GetBytes(UsingExpression);

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE + moduleStatePacket.Length +  usingEyePacket.Length + usingExpressionPacket.Length;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC,   0, finalDataStream, 0, SIZE_PACKET_MAGIC);          // Magic
        Buffer.BlockCopy(packetTypeBytes,   0, finalDataStream, 4, SIZE_PACKET_TYPE);           // Packet Type
        Buffer.BlockCopy(moduleStatePacket, 0, finalDataStream, 8, moduleStatePacket.Length);   // Module State
        Buffer.BlockCopy(usingEyePacket,   0, finalDataStream, 12, usingEyePacket.Length);       // Using Eye
        Buffer.BlockCopy(usingExpressionPacket, 0, finalDataStream, 13, usingExpressionPacket.Length);   // Using Expression

        return finalDataStream;
    }

    public override void Decode(in byte[] data)
    {
        ModuleState = (ModuleState) BitConverter.ToInt32(data, 8);
        UsingEye = BitConverter.ToBoolean(data, 12);
        UsingExpression = BitConverter.ToBoolean(data, 13);
    }
}
