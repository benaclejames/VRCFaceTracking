namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class DnsResource : DnsQuestion
{
    private static readonly Dictionary<ushort, Type> _typeMap = new Dictionary<ushort, Type>
    {
        {1, typeof(ARecord)},
        {2, typeof(NSRecord)},
        {12, typeof(PTRRecord)},
        {16, typeof(TXTRecord)},
        {33, typeof(SRVRecord)},
    };
        
    public TimeSpan TTL;
    public IDnsSerializer Data;

    public DnsResource(BigReader reader) : base(reader)
    {
        TTL = TimeSpan.FromSeconds(reader.ReadUInt32());
            
        var dataLength = (int)reader.ReadUInt16();
        var expectedEnd = reader.BaseStream.Position + dataLength;
            
        if (_typeMap.TryGetValue(Type, out var type))
        {
            Data = (IDnsSerializer)Activator.CreateInstance(type);
            Data.Deserialize(reader, dataLength);
        }
        else
        {
            reader.ReadBytes(dataLength);
        }

        if (reader.BaseStream.Position != expectedEnd)
            throw new Exception("Invalid resource record");
    }

    public DnsResource(IDnsSerializer data, List<string> names) : base(names, _typeMap.First(x => x.Value == data.GetType()).Key, 1)
    {
        Data = data;
        TTL = TimeSpan.FromSeconds(120);
    }
 
    public override byte[] Serialize()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(base.Serialize());
            
        bytes.AddRange(BigWriter.WriteUInt32((uint)TTL.TotalSeconds));
            
        var data = Data.Serialize();
        bytes.AddRange(BigWriter.WriteUInt16((ushort)data.Length));
        bytes.AddRange(data);
            
        return bytes.ToArray();
    }
}