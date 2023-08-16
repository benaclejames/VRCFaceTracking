using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.OSC;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VRCFaceTracking
{
    public class ConfigParser
    {
        private static ILogger<ConfigParser> _logger;

        public ConfigParser(ILogger<ConfigParser> parserLogger)
        {
            _logger = parserLogger;
        }

        public class InputOutputDef
        {
            public string address { get; set; }
            public string type { get; set; }

            [System.Text.Json.Serialization.JsonIgnore]
            public Type Type => OscUtils.TypeConversions.Where(conversion => conversion.Value.configType == type).Select(conversion => conversion.Key.Item1).FirstOrDefault();
        }

        public class Parameter
        {
            public string name { get; set; }
            public InputOutputDef input { get; set; }
            public InputOutputDef output { get; set; }
        }

        public class AvatarConfigSpec
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<Parameter> parameters { get; set; }
        }

        public static Action<IParameter[], string, string> OnConfigLoaded = (_, _, _) => { };
        public static string AvatarId = "";
        private HttpClient _httpClient = new();

        public void ParseFromFile(string newId)
        {
            if (newId == AvatarId || string.IsNullOrEmpty(newId))
                return;

            var paramList = new List<IParameter>();
            
            if (newId.StartsWith("local:"))
            {
                foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                    paramList.AddRange(parameter.ResetParam(Array.Empty<(string paramName, string paramAddress, Type paramType)>()));

                // This is a local test avatar, there won't be a config file for it so assume we're using no parameters and just return
                OnConfigLoaded(paramList.ToArray(), newId, newId.Substring(10));
                AvatarId = newId;
                return;
            }
            
            AvatarConfigSpec avatarConfig = null;
            foreach (var userFolder in Directory.GetDirectories(VRChat.VRCOSCDirectory))
            {
                if (Directory.Exists(userFolder + "\\Avatars"))
                    foreach (var avatarFile in Directory.GetFiles(userFolder+"\\Avatars"))
                    {
                        var configText = File.ReadAllText(avatarFile);
                        var tempConfig = JsonSerializer.Deserialize<AvatarConfigSpec>(configText);
                        if (tempConfig == null || tempConfig.id != newId)
                            continue;
                    
                        avatarConfig = tempConfig;
                        break;
                    }
            }

            if (avatarConfig == null)
            {
                _logger.LogError("Avatar config file for " + newId + " not found");
                return;
            }

            _logger.LogInformation("Parsing config file for avatar: " + avatarConfig.name);
            ParamSupervisor.SendQueue.Clear();
            var parameters = avatarConfig.parameters.Where(param => param.input != null)
                .Select(param => (param.name, param.input.address, param.input.Type)).ToArray();

            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                paramList.AddRange(parameter.ResetParam(parameters));

            OnConfigLoaded(paramList.ToArray(), avatarConfig.id, avatarConfig.name);
            AvatarId = newId;
        }

        public async void ParseFromOscQuery(IPEndPoint oscQueryEndpoint)
        {
            // Request on the endpoint + /avatar/parameters
            var httpEndpoint = "http://" + oscQueryEndpoint + "/avatar";
            
            // Get the response
            var request = await _httpClient.GetAsync(httpEndpoint);
            if (!request.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get avatar parameters from " + httpEndpoint);
                return;
            }
            
            // Parse the response
            var response = await request.Content.ReadAsStringAsync();
            var avatarConfig = JsonConvert.DeserializeObject<OSCQueryNode>(response);
            
            var targetParams = avatarConfig.Contents["parameters"].Contents.Select(entry => 
                (entry.Key, entry.Value.FullPath, OscUtils.TypeConversions.First(t => t.Key.typeChar.Contains(entry.Value.OscType.First())).Key.Item1)).ToArray();
            
            // Reset all parameters
            var paramList = new List<IParameter>();
            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                paramList.AddRange(parameter.ResetParam(targetParams));

            var newId = avatarConfig.Contents["change"].Value[0] as string;
            OnConfigLoaded(paramList.ToArray(), newId, "Unknown Name (half baked oscquery impl yaaay)");
            AvatarId = newId;
        }
    }
}