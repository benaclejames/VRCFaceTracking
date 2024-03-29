using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Core
{
    public class OscQueryConfigParser
    {
        private readonly ILogger<OscQueryConfigParser> _logger;
        private readonly AvatarConfigParser _configParser;

        public OscQueryConfigParser(ILogger<OscQueryConfigParser> parserLogger, AvatarConfigParser configParser)
        {
            _logger = parserLogger;
            _configParser = configParser;
        }

        private readonly HttpClient _httpClient = new();

        public async Task<(IAvatarInfo avatarInfo, List<Parameter> relevantParameters)?> ParseNewAvatar(IPEndPoint oscQueryEndpoint)
        {
            try
            {
                // Request on the endpoint + /avatar/parameters
                var httpEndpoint = "http://" + oscQueryEndpoint + "/avatar";

                // Get the response
                var response = await _httpClient.GetAsync(httpEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var avatarConfig =
                    JsonConvert.DeserializeObject<OSCQueryNode>(await response.Content.ReadAsStringAsync());
                var avatarInfo = new OscQueryAvatarInfo(avatarConfig);

                // Reset all parameters
                var paramList = new List<Parameter>();
                foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1)
                             .ToArray())
                {
                    paramList.AddRange(parameter.ResetParam(avatarInfo.Parameters));
                }
                
                // God help me why is this something I need to do to get the avatar name
                // this impl is really disappointing vrc
                var configFileInfo = await _configParser.ParseNewAvatar(avatarInfo.Id);
                _logger.LogInformation($"Attempting to resolve avatar config file for {avatarInfo.Id}");
                if (!string.IsNullOrEmpty(configFileInfo?.avatarInfo.Name))
                {
                    avatarInfo.Name = configFileInfo.Value.avatarInfo.Name;
                    _logger.LogInformation($"Successfully found config containing avatar name {avatarInfo.Name}");
                }
                else
                {
                    _logger.LogWarning("Odd. Out attempt to find the legacy osc config json for this avatar failed.");
                }

                return (avatarInfo, paramList);
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e, scope => scope.SetExtra("endpoint", oscQueryEndpoint));
                return null;
            }
        }
    }
}