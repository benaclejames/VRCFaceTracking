namespace VRCFaceTracking.Core.OSC.Query.mDNS;

// Big endian reader
public class BigReader : BinaryReader
{
    private Dictionary<int, List<string>> nameCache = new Dictionary<int, List<string>>();
        
    public BigReader(byte[] data) : base(new MemoryStream(data)) { }
        
    public override ushort ReadUInt16() => (ushort)((base.ReadByte() << 8) | base.ReadByte());
        
    public override uint ReadUInt32() => (uint)((base.ReadByte() << 24) | (base.ReadByte() << 16) | (base.ReadByte() << 8) | base.ReadByte());

    public override string ReadString()
    {
        var length = ReadByte();
        var data = ReadBytes(length);
        return System.Text.Encoding.ASCII.GetString(data);
    }

    // Referenced from https://github.com/meamod/MeaMod.DNS/blob/master/src/Model/WireReader.cs#L189
    public List<string> ReadDomainLabels()
    {
        var pointer = (int)BaseStream.Position;
        var length = ReadByte();
        if ((length & 0xC0) == 0xC0)
        {
            var ptr = (length ^ 0xC0) << 8 | ReadByte();
            var cname = nameCache[ptr];
            nameCache[pointer] = cname;
            return cname;
        }
            
        var labels = new List<string>();
        if (length == 0)
            return labels;
            
        var data = ReadBytes(length);
        labels.Add(System.Text.Encoding.UTF8.GetString(data, 0, length));
        labels.AddRange(ReadDomainLabels());
        nameCache[pointer] = labels;
            
        return labels;
    }
}