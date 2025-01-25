using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.mDNS;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core;

public class OscQueryConfigParser
{
    private readonly ILogger<OscQueryConfigParser> _logger;
    private readonly AvatarConfigParser _configParser;
    private readonly MulticastDnsService _multicastDnsService;

    public OscQueryConfigParser(
        ILogger<OscQueryConfigParser> parserLogger, 
        AvatarConfigParser configParser, 
        MulticastDnsService multicastDnsService
    )
    {
        _logger = parserLogger;
        _configParser = configParser;
        _multicastDnsService = multicastDnsService;
    }

    private readonly HttpClient _httpClient = new();

    public async Task<(IAvatarInfo avatarInfo, List<Parameter> relevantParameters)?> ParseAvatar(string avatarId = null)
    {
        try
        {
            if (_multicastDnsService.VrchatClientEndpoint == null)
            {
                return null;
            }
            
            // Request on the endpoint + /avatar/parameters
            var httpEndpoint = "http://" + _multicastDnsService.VrchatClientEndpoint + "/avatar";

            // Get the response
            var response = await _httpClient.GetAsync(httpEndpoint);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var avatarConfig =
                JsonConvert.DeserializeObject<OscQueryNode>(await response.Content.ReadAsStringAsync());
            _logger.LogDebug(avatarConfig.ToString());
            var avatarInfo = new OscQueryAvatarInfo(avatarConfig);

            // Reset all parameters
            var paramList = new List<Parameter>();
            foreach (var parameter in UnifiedTracking.AllParameters)
            {
                paramList.AddRange(parameter.ResetParam(avatarInfo.Parameters));
            }
                
            // God help me why is this something I need to do to get the avatar name
            // this impl is really disappointing vrc
            var configFileInfo = await _configParser.ParseAvatar(avatarInfo.Id);
            _logger.LogInformation($"Attempting to resolve avatar config file for {avatarInfo.Id}");
            if (!string.IsNullOrEmpty(configFileInfo?.avatarInfo.Name))
            {
                avatarInfo.Name = configFileInfo.Value.avatarInfo.Name;
                _logger.LogInformation($"Successfully found config containing avatar name {avatarInfo.Name}");
            }
            else
            {
                _logger.LogWarning("Odd. Our attempt to find the legacy osc config json for this avatar failed.");
            }

            return (avatarInfo, paramList);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            SentrySdk.CaptureException(e, scope => scope.SetExtra("endpoint", _multicastDnsService.VrchatClientEndpoint));
            return null;
        }
    }
}