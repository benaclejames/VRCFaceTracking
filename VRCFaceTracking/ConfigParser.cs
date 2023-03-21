using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking
{
    public static class ConfigParser
    {
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

        public static Action<int> OnConfigLoaded = (int count) => { };

        public static void ParseNewAvatar(string newId)
        {
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
                Logger.Error("Avatar config file for " + newId + " not found");
                return;
            }
            
            Logger.Msg("Parsing config file for avatar: " + avatarConfig.name);
            var parameters = avatarConfig.parameters.Where(param => param.input != null).ToArray();

            int count = 0;
            foreach (var parameter in UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray())
                count += parameter.ResetParam(parameters);

            OnConfigLoaded(count);
        }
    }
}