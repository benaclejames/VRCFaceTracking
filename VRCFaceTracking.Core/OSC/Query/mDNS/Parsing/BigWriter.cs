using System.Text;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public static class BigWriter
{
    public static byte[] WriteUInt16(ushort value)
    {
        byte[] bytes = new byte[2];
        bytes[0] = (byte)(value >> 8);
        bytes[1] = (byte)(value & 0xFF);
        return bytes;
    }
        
    public static byte[] WriteUInt32(uint value)
    {
        byte[] bytes = new byte[4];
        bytes[0] = (byte)(value >> 24);
        bytes[1] = (byte)(value >> 16);
        bytes[2] = (byte)(value >> 8);
        bytes[3] = (byte)(value & 0xFF);
        return bytes;
    }
        
    public static byte[] WriteDomainLabels(List<string> labels)
    {
        List<byte> bytes = new List<byte>();
        foreach (string label in labels)
        {
            bytes.Add((byte)label.Length);
            bytes.AddRange(Encoding.ASCII.GetBytes(label));
        }
        bytes.Add(0);
        return bytes.ToArray();
    }
}