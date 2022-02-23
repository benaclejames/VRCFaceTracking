using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VRCFaceTracking
{
    public static class ConfigParser
    {
        public class InputOutputDef
        {
            public string address { get; set; }
            public string type { get; set; }

            [JsonIgnore]
            public Type Type
            {
                get
                {
                    switch (type)
                    {
                        case "Bool":
                            return typeof(bool);
                        case "Float":
                            return typeof(float);
                        default:
                            throw new Exception("Unknown type");
                    }
                }
            }
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

        public static Action OnConfigLoaded = () => { };

        public static void ParseNewAvatar(string path)
        {
            // Read the file
            var avatarConfig = File.ReadAllText(path);
            
            // Parse the config file to json
            var avatarConfigSpec = JsonSerializer.Deserialize<AvatarConfigSpec>(avatarConfig);

            if (avatarConfigSpec == null)
                return;
            
            Logger.Msg("Parsing config file for avatar: " + avatarConfigSpec.name);
            var parameters = avatarConfigSpec.parameters.Where(param => param.input != null).ToArray();
            foreach (var parameter in UnifiedTrackingData.AllParameters)
                parameter.ResetParam(parameters);

            OnConfigLoaded();
        }
    }
}