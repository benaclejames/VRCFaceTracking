using System.Text;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class TXTRecord : IDnsSerializer
{
    public List<string> Text;
        
    public TXTRecord()
    {
        Text = new List<string>();
    }
        
    public byte[] Serialize()
    {
        // Serialize the text to bytes
        List<byte> bytes = new List<byte>();
        foreach (string s in Text)
        {
            bytes.Add((byte)s.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(s));
        }
        return bytes.ToArray();
    }

    public void Deserialize(BigReader reader, int expectedLength)
    {
        Text = new List<string>();
        while (expectedLength > 0)
        {
            var currString = reader.ReadString();
            Text.Add(currString);
            expectedLength -= Encoding.ASCII.GetByteCount(currString) + 1;
        }
    }
}