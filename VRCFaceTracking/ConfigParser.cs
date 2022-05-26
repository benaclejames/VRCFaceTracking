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
        public class StringToTypeConverter : JsonConverter<Type>
        {
            public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Read the string value
                string value = reader.GetString();
                return Utils.TypeConversions.First(elem => elem.Value.configType == value).Key;
            }

            public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
            {
            }
        }
        
        public class InputOutputDef
        {
            public string address { get; set; }
            [JsonConverter(typeof(StringToTypeConverter))]
            public Type type { get; set; } 
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

        public static void ParseNewAvatar(string newId)
        {
            AvatarConfigSpec avatarConfig = null;
            foreach (var userFolder in Directory.GetDirectories(Utils.VRCOSCDirectory))
            {
                var avatarFolder = userFolder+"\\Avatars\\";
                if (!Directory.Exists(userFolder))
                {
                    continue;
                }
                try
                {
                    foreach (var avatarFile in Directory.GetFiles(avatarFolder))
                    {
                        var configText = File.ReadAllText(avatarFile);
                        try
                        {
                            var tempConfig = JsonSerializer.Deserialize<AvatarConfigSpec>(configText);
                            if (tempConfig == null || tempConfig.id != newId)
                                continue;

                            avatarConfig = tempConfig;
                            break;
                        }
                        catch (JsonException e)
                        {
                            Logger.Warning("Failed to parse JSON file: "+avatarFile+". Ensure it follows RFC 8259 formatting");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warning("Failed to parse folder: " + avatarFolder);
                }
            }
                

            if (avatarConfig == null)
            {
                Logger.Error("Avatar config file for " + newId + " not found");
                return;
            }
            
            Logger.Msg("Parsing config file for avatar: " + avatarConfig.name);
            var parameters = avatarConfig.parameters.Where(param => param.input != null).ToArray();
            foreach (var parameter in UnifiedTrackingData.AllParameters)
                parameter.ResetParam(parameters);

            OnConfigLoaded();
        }
    }
}