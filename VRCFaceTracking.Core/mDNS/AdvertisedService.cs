using System.Net;

namespace VRCFaceTracking.Core.mDNS;

public record AdvertisedService(string ServiceName, int Port, IPAddress Address)
{
    public readonly string ServiceName = ServiceName;
    public readonly int Port = Port;
    public readonly IPAddress Address = Address;
}