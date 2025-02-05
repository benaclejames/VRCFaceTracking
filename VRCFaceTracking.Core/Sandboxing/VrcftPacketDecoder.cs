using VRCFaceTracking.Core.Sandboxing.IPC;

namespace VRCFaceTracking.Core.Sandboxing;
public class VrcftPacketDecoder
{
    /// <summary>
    /// Attempts to decode an incoming stream of bytes into an IpcPacket structure
    /// </summary>
    /// <param name="data">The incoming byte stream</param>
    /// <param name="packet">The output packet, if successfully decoded</param>
    /// <returns>Whether decoding was successful. If it was not, the data stored in packet should be discarded.</returns>
    public static bool TryDecodePacket(in byte[] data, out IpcPacket packet)
    {
        packet = new IpcPacket();

        // Verify magic numbers
        for ( int i = 0; i < IpcPacket.SIZE_PACKET_TYPE; i++ )
        {
            if ( data[i] != IpcPacket.HANDSHAKE_MAGIC[i] )
            {
                return false;
            }
        }

        // Read packet type
        IpcPacket.PacketType packetType = (IpcPacket.PacketType) (BitConverter.ToUInt32(data, 4));

        // Based on packet type, switch to specific packet decoder
        switch ( packetType )
        {
            // Handshake
            case IpcPacket.PacketType.Handshake:
                packet = new HandshakePacket();
                packet.Decode(data);
                break;

            // SplitPacketChunk
            case IpcPacket.PacketType.SplitPacketChunk:
                packet = new PartialPacket();
                packet.Decode(data);
                break;


            // EventGetSupported
            case IpcPacket.PacketType.EventGetSupported:
                packet = new EventInitGetSupported();
                packet.Decode(data);
                break;

            // EventInit
            case IpcPacket.PacketType.EventInit:
                packet = new EventInitPacket();
                packet.Decode(data);
                break;

            // EventTeardown
            case IpcPacket.PacketType.EventTeardown:
                packet = new EventTeardownPacket();
                packet.Decode(data);
                break;

            // EventUpdate
            case IpcPacket.PacketType.EventUpdate:
                packet = new EventUpdatePacket();
                packet.Decode(data);
                break;

            // EventLog
            case IpcPacket.PacketType.EventLog:
                packet = new EventLogPacket();
                packet.Decode(data);
                break;



            // EventGetSupported
            case IpcPacket.PacketType.ReplyGetSupported:
                packet = new ReplySupportedPacket();
                packet.Decode(data);
                break;

            // ReplyInit
            case IpcPacket.PacketType.ReplyInit:
                packet = new ReplyInitPacket();
                packet.Decode(data);
                break;

            // ReplyUpdate
            case IpcPacket.PacketType.ReplyUpdate:
                packet = new ReplyUpdatePacket();
                packet.Decode(data);
                break;

            // ReplyTeardown
            case IpcPacket.PacketType.ReplyTeardown:
                packet = new ReplyTeardownPacket();
                packet.Decode(data);
                break;

            // EventUpdateStatus
            case IpcPacket.PacketType.EventUpdateStatus:
                packet = new EventStatusUpdatePacket();
                packet.Decode(data);
                break;

            // Invalid packet
            case IpcPacket.PacketType.Unknown:
            default:
#if DEBUG
                throw new NotImplementedException($"No decoder for packet type {packetType} implemented in VrcftPacketDecoder! Packets of this type will be ignored.");
#else
                return false;
#endif
        } 

        return true;
    }
}
