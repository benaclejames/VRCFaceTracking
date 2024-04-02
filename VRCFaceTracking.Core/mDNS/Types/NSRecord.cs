namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class NSRecord : IDnsSerializer
{
    public List<string> Authority;
    
    public byte[] Serialize() => throw new NotImplementedException();

    public void Deserialize(BigReader reader, int expectedLength)
    {
        Authority = reader.ReadDomainLabels();
    }
}