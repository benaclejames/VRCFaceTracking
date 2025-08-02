using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.mDNS;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.Params;

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

            // Request on the endpoint + /avatar/parameters
            var httpEndpoint = "http://" + multicastDnsService.VrchatClientEndpoint + "/avatar";

            // Get the response
            var response = await _httpClient.GetAsync(httpEndpoint);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var avatarConfig =
                JsonConvert.DeserializeObject<OscQueryNode>(await response.Content.ReadAsStringAsync());
            parserLogger.LogDebug(avatarConfig.ToString());
            var avatarInfo = new OscQueryAvatarInfo(avatarConfig);

            // Reset all parameters
            var paramList = new List<Parameter>();
            foreach (var parameter in UnifiedTracking.AllParameters)
            {
                paramList.AddRange(parameter.ResetParam(avatarInfo.Parameters));
            }

            // God help me why is this something I need to do to get the avatar name
            // this impl is really disappointing vrc
            var configFileInfo = await configParser.ParseAvatar(avatarInfo.Id);
            parserLogger.LogInformation($"Attempting to resolve avatar config file for {avatarInfo.Id}");
            if (!string.IsNullOrEmpty(configFileInfo?.avatarInfo.Name))
            {
                avatarInfo.Name = configFileInfo.Value.avatarInfo.Name;
                parserLogger.LogInformation($"Successfully found config containing avatar name {avatarInfo.Name}");
            }
            else
            {
                parserLogger.LogWarning("Odd. Our attempt to find the legacy osc config json for this avatar failed.");
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
