using System.Net;

namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class ARecord : IDnsSerializer
{
    public IPAddress Address;
        
    public ARecord()
    {
            
    }
        
    public byte[] Serialize()
    {
        return Address.GetAddressBytes();
    }

    public void Deserialize(BigReader reader, int expectedLength)
    {
        Address = new IPAddress(reader.ReadBytes(expectedLength));
    }
}