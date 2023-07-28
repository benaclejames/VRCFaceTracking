﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.OSC;

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

            [JsonIgnore]
            public Type Type => OscUtils.TypeConversions.Where(conversion => conversion.Value.configType == type).Select(conversion => conversion.Key).FirstOrDefault();
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

        public static Action<IParameter[], AvatarConfigSpec> OnConfigLoaded = (_, _) => { };
        public static string AvatarId = "";

        public void ParseNewAvatar(string newId)
        {
            if (newId == AvatarId || string.IsNullOrEmpty(newId))
                return;

            var paramList = new List<IParameter>();
            
            if (newId.StartsWith("local:"))
            {
                foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                    paramList.AddRange(parameter.ResetParam(Array.Empty<Parameter>()));

                // This is a local test avatar, there won't be a config file for it so assume we're using no parameters and just return
                OnConfigLoaded(paramList.ToArray(), new AvatarConfigSpec()
                {
                    id = newId,
                    name = newId.Substring(10) // Remove "local:sdk_" from the name
                });
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
            var parameters = avatarConfig.parameters.Where(param => param.input != null).ToArray();

            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                paramList.AddRange(parameter.ResetParam(parameters));

            OnConfigLoaded(paramList.ToArray(), avatarConfig);
            AvatarId = newId;
        }
    }
}