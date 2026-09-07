using System.Security.Cryptography;
using System.Text;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Services;

public class IdentityService : IIdentityService
{
    private string _uniqueUserId = string.Empty;
    
    public string GetUniqueUserId()
    {
        // Updated to support multi platform, so old ratings are gonna be lost
        if (!string.IsNullOrEmpty(_uniqueUserId))
        {
            return _uniqueUserId;
        }

        var systemId = Environment.MachineName + Environment.UserName + Environment.OSVersion.Platform;
        var machineIdBytes = Encoding.UTF8.GetBytes(systemId);
        var hashed = SHA256.HashData(machineIdBytes);
        _uniqueUserId = Convert.ToHexString(hashed);

        return _uniqueUserId;
    }
}