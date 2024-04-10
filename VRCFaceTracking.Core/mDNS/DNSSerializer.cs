namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public interface IDnsSerializer
{
    byte[] Serialize();
    void Deserialize(BigReader reader, int expectedLength);
}