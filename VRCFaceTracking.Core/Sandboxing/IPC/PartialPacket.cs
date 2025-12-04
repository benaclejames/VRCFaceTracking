using System.Diagnostics;

namespace VRCFaceTracking.Core.Sandboxing.IPC;

/// <summary>
/// Special type of packet. This packet is a series of bytes which will be re-assembled in order once all of them have been received.
/// </summary>
public class PartialPacket : IpcPacket 
{
    public override PacketType GetPacketType() => PacketType.SplitPacketChunk;

    // Bit-wise mask on packetPart
    private const uint PACKET_PART_EOF_BIT = 0x80000000;
    // Size in bytes of packet header
    private const uint PACKET_NON_DATA_SIZE_BYTES = 16;
    private const uint PACKET_COUNT_INVALID = 0xFFFFFFFF;

    // Global counter for which partial packet we are encoding
    private static uint _encodePartialPacketCounter = 0;
    private static uint _encodePartialPacketBase = 0;
    private static Dictionary<uint, PartialPacketChunkTemporaryBuffer> _packetBuffers = new();

    private struct PartialPacketChunk
    {
        public byte[] Data;
        public uint PacketPart;
    }
    private class PartialPacketChunkTemporaryBuffer
    {
        public List<PartialPacketChunk> Chunks = new();
        public uint PacketCount = PACKET_COUNT_INVALID;
    }

    // Unused for partial packet
    public override byte[] GetBytes() { return Array.Empty<byte>(); }
    public override void Decode(in byte[] data) {}

    public static void DecodePacket(byte[] data, out byte[] packetData)
    {
        packetData = new byte[0];

        // Get packetID and packet part out of data
        uint packetId   = BitConverter.ToUInt32(data, 12);
        uint packetPart = BitConverter.ToUInt32(data, 8);
        Debug.WriteLine($"Received frame of {data.Length} bytes with ID {packetId} and part {packetPart}!");

        var packetChunk = new PartialPacketChunk()
        {
            PacketPart = packetPart & ~PACKET_PART_EOF_BIT, // Mask out the EOF bit if it's set
            Data = new byte[data.Length - PACKET_NON_DATA_SIZE_BYTES]
        };

        Buffer.BlockCopy(data, ( int )PACKET_NON_DATA_SIZE_BYTES, packetChunk.Data, 0, ( int )( data.Length - PACKET_NON_DATA_SIZE_BYTES ));

        if ( _packetBuffers.ContainsKey(packetId) )
        {
            _packetBuffers[packetId].Chunks.Add(packetChunk);
        }
        else
        {
            var packetChunkCollection = new PartialPacketChunkTemporaryBuffer();
            packetChunkCollection.Chunks.Add(packetChunk);
            _packetBuffers.Add(packetId, packetChunkCollection);
        }

        if ( (packetPart & PACKET_PART_EOF_BIT) == PACKET_PART_EOF_BIT )
        {
            // If this is an EOF packet, mask out all bits other than the EOF bit
            packetPart = packetPart & ~PACKET_PART_EOF_BIT;

            // Since this is an EOF packet we know how many chunks this packet is composed of
            _packetBuffers[packetId].PacketCount = packetPart + 1;

            // Check if we have received all the packets we're supposed to have received for this packet
            if ( _packetBuffers[packetId].PacketCount == _packetBuffers[packetId].Chunks.Count )
            {
                goto combinePackets;
            }
        }

        // Now if we know the amount of packets we are expected to have for this packet but have received a non-EOF packet,
        // this packet could be the last packet we need to construct the entire packet:
        //    i.e. We received the EOF packet before, and have received antoher packet belonging to the same packetId
        if ( _packetBuffers[packetId].PacketCount != PACKET_COUNT_INVALID )
        {
            // Check if we have received all the packets necessary to reconstruct the original packet
            if ( _packetBuffers[packetId].PacketCount == _packetBuffers[packetId].Chunks.Count )
            {
                goto combinePackets;
            }
        }

        return;
        
    combinePackets:

        // We have received all packets! Allocate some memory to combine them into before we return
        long computedPacketSize = 0;
        for ( int i = 0; i < _packetBuffers[packetId].Chunks.Count; i++ )
        {
            computedPacketSize += _packetBuffers[packetId].Chunks[i].Data.Length;
        }
        packetData = new byte[computedPacketSize];

        // Copy the data to packetData
        int copyOffset = 0;
        // We iterate by packetPart. This should solve packets arriving in an out-of-order fashion.
        for ( int copiedPartCounter = 0; copiedPartCounter < _packetBuffers[packetId].Chunks.Count; copiedPartCounter++ )
        {
            for ( int i = 0; i < _packetBuffers[packetId].Chunks.Count; i++ )
            {
                // Search for the chunk containing the current packetPart we are looking for
                if ( _packetBuffers[packetId].Chunks[i].PacketPart == copiedPartCounter )
                {
                    Buffer.BlockCopy(_packetBuffers[packetId].Chunks[i].Data, 0, packetData, copyOffset, _packetBuffers[packetId].Chunks[i].Data.Length);
                    copyOffset += _packetBuffers[packetId].Chunks[i].Data.Length;
                    
                    break; // Exit out of inner loop
                }
            }
        }

        Console.WriteLine($"Decoded packet of size {computedPacketSize}!");

        return;
    }

    public static byte[][] SplitPacketIntoChunks(byte[] packetData, int mtu)
    {
        // Decrease the MTU by 128 bytes if possible, for good measure
        if ( mtu > 128 )
        {
            mtu -= 128;
        }
        Console.WriteLine($"[INFO]: Splitting packet of size {packetData.Length} by MTU {mtu}...");
        List<byte[]> packetCollection = new List<byte[]>();

        // Determine the amount of packets we need to create.
        // Each packet has an overhead of 16 bytes.
        long partialPacketCount = (long)Math.Ceiling((double)packetData.Length / (mtu - PACKET_NON_DATA_SIZE_BYTES));

        // Determine the packet ID.
        // To try giving this process a unique ID we take the process ID, square it and append the packet counter into it
        // We also add a constant random number to the offset.
        // We then also XOR the packet ID with a counter.
        // @TODO: Test this algorithm for collisions. This should effectively guarantee unique numbers regardless of which connection it is.
        if ( _encodePartialPacketBase == 0 )
        {
            _encodePartialPacketBase = ( uint )(( Process.GetCurrentProcess().Id * Process.GetCurrentProcess().Id ) + new Random().Next(0, 10000));
        }
        uint packetId = _encodePartialPacketBase ^ _encodePartialPacketCounter;
        _encodePartialPacketCounter++;

        byte[] packetTypeBytes = BitConverter.GetBytes((uint)PacketType.SplitPacketChunk);
        byte[] packetIdBytes = BitConverter.GetBytes(packetId);

        // Keep track of how many packets we've encoded
        int bytesLeftToPack = packetData.Length;
        int bytesSent = 0;

        Console.WriteLine($"[INFO]: Packet count {partialPacketCount}");
        for ( uint i = 0; i < partialPacketCount; i++ )
        {
            // The size of a packet has a maximum size of MTU
            int packetSize = Math.Min((int)(bytesLeftToPack + PACKET_NON_DATA_SIZE_BYTES), mtu);
            int packetSizeNoOverhead = ( int )Math.Max(packetSize - PACKET_NON_DATA_SIZE_BYTES, 0);

            // Create each partial packet
            uint packetPart = i;
            if ( i == partialPacketCount - 1 )
            {
                // Set EOF bit if this is the last packet
                packetPart = packetPart | PACKET_PART_EOF_BIT;
            }
            byte[] packetPartBytes = BitConverter.GetBytes(packetPart);

            Console.WriteLine($"packetSize[{i}] => {packetSize}");
            Console.WriteLine($"packetSizeNoOverhead[{i}] => {packetSizeNoOverhead}");

            // Prepare buffer
            byte[] finalDataStream = new byte[packetSize];
            Buffer.BlockCopy(HANDSHAKE_MAGIC, 0, finalDataStream, 0,  SIZE_PACKET_MAGIC);       // Magic
            Buffer.BlockCopy(packetTypeBytes, 0, finalDataStream, 4,  SIZE_PACKET_TYPE);        // Packet Type
            Buffer.BlockCopy(packetPartBytes, 0, finalDataStream, 8,  sizeof(int));             // Packet Part
            Buffer.BlockCopy(packetIdBytes,   0, finalDataStream, 12, sizeof(int));             // Packet ID

            // Copy actual data
            Buffer.BlockCopy(packetData, bytesSent, finalDataStream, 16, packetSizeNoOverhead); // Byte chunk

            bytesSent += packetSizeNoOverhead;
            bytesLeftToPack -= packetSizeNoOverhead;

            Console.WriteLine($"bytesSent[{i}] => {bytesSent}");
            Console.WriteLine($"bytesLeftToPack[{i}] => {bytesLeftToPack}");

            packetCollection.Add(finalDataStream);
        }

        long packedDataBytes = 0;
        foreach ( var elem in packetCollection )
        {
            packedDataBytes += elem.Length - PACKET_NON_DATA_SIZE_BYTES;
        }
        Console.WriteLine($"[INFO]: Split {packetData.Length} bytes into {packetCollection.Count} partial packets (combined size: {packedDataBytes})!");

        return packetCollection.ToArray();
    }
}
