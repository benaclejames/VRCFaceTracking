namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public interface DNSSerializer
{
    byte[] Serialize();
    void Deserialize(BigReader reader, int expectedLength);
}