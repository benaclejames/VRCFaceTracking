using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking
{
    public class OscQueryConfigParser
    {
        private static ILogger<OscQueryConfigParser> _logger;

        public OscQueryConfigParser(ILogger<OscQueryConfigParser> parserLogger)
        {
            _logger = parserLogger;
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