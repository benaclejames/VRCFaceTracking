namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class SRVRecord : IDnsSerializer
{
    public ushort Priority;
    public ushort Weight;
    public ushort Port;
    public List<string> Target;

    public byte[] Serialize()
    {
        // Serialize the SRV record
        List<byte> bytes = new List<byte>();
            
        // Write priority as big endian ushort
        bytes.AddRange(BigWriter.WriteUInt16(Priority));
            
        // Write weight as big endian ushort
        bytes.AddRange(BigWriter.WriteUInt16(Weight));
            
        // Write port as big endian ushort
        bytes.AddRange(BigWriter.WriteUInt16(Port));
            
        // Write target
        bytes.AddRange(BigWriter.WriteDomainLabels(Target));
            
        return bytes.ToArray();
    }

    public void Deserialize(BigReader reader, int expectedLength)
    {
        Priority = reader.ReadUInt16();
        Weight = reader.ReadUInt16();
        Port = reader.ReadUInt16();
        Target = reader.ReadDomainLabels();
    }
}