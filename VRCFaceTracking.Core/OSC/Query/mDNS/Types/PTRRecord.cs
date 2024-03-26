namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class PTRRecord : DNSSerializer
{
    public List<string> DomainLabels;
        
    public PTRRecord()
    {
        DomainLabels = new List<string>();
    }
        
    public byte[] Serialize()
    {
        return BigWriter.WriteDomainLabels(DomainLabels);
    }

    public void Deserialize(BigReader reader, int expectedLength)
    {
        DomainLabels = reader.ReadDomainLabels();
    }
}