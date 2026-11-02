using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.mDNS;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core;

public class OscQueryConfigParser(
    ILogger<OscQueryConfigParser> parserLogger,
    AvatarConfigParser configParser,
    MulticastDnsService multicastDnsService)
{
    private readonly HttpClient _httpClient = new();

    public async Task<(IAvatarInfo avatarInfo, List<Parameter> relevantParameters)?> ParseAvatar(string avatarId = null)
    {
        try
        {
            if (multicastDnsService.VrchatClientEndpoint == null)
            {
                return null;
            }

            var httpEndpoint = new Uri("http://" + multicastDnsService.VrchatClientEndpoint + "/avatar");
            var avatarConfig = await OscQueryAvatarReader.ReadAsync(_httpClient, httpEndpoint);
            
            ParameterSenderService.Clear();
            parserLogger.LogDebug(avatarConfig.ToString());
            var avatarInfo = new OscQueryAvatarInfo(avatarConfig);

            // Reset all parameters
            var paramList = new List<Parameter>();
            foreach (var parameter in UnifiedTracking.AllParameters)
            {
                // We pass an empty array if we're using fft. This will never happen naturally so we can use it as confirmation of such an event
                paramList.AddRange(parameter.ResetParam(avatarInfo.Parameters, avatarInfo));
            }

            return (avatarInfo, paramList);
        }
        catch (Exception e)
        {
            parserLogger.LogError(e.Message);
            SentrySdk.CaptureException(e, scope => scope.SetExtra("endpoint", multicastDnsService.VrchatClientEndpoint));
            return null;
        }
    }
}
