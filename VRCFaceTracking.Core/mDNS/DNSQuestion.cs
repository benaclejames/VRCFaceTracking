namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class DnsQuestion
{
    public List<string> Labels;
    public ushort Type;
    public ushort Class;
        
    public DnsQuestion(BigReader reader)
    {
        Labels = reader.ReadDomainLabels();
        Type = reader.ReadUInt16();
        Class = reader.ReadUInt16();
    }
        
    public DnsQuestion(List<string> labels, ushort type, ushort @class)
    {
        Labels = labels;
        Type = type;
        Class = @class;
    }

    public virtual byte[] Serialize()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BigWriter.WriteDomainLabels(Labels));
        bytes.AddRange(BigWriter.WriteUInt16(Type));
        bytes.AddRange(BigWriter.WriteUInt16(Class));
        return bytes.ToArray();
    }
}