using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public static bool TryDecodePacket(ref byte[] data, out IpcPacket packet)
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
                packet.Decode(ref data);
                break;

            // Invalid packet
            case IpcPacket.PacketType.Unknown:
            default:
                return false;
        } 

        return true;
    }
}
