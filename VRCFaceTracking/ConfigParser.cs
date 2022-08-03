﻿using System;
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
            public Type Type => Utils.TypeConversions.Where(conversion => conversion.Value.configType == type).Select(conversion => conversion.Key).FirstOrDefault();
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

        public static AvatarConfigSpec LoadConfigFrom(string path, string avatarPath, string avatarId)
        {
            AvatarConfigSpec avatarConfig = null;
            
            foreach (var userFolder in Directory.GetDirectories(path))
            {
                if (Directory.Exists(userFolder + avatarPath))
                {
                    foreach (var avatarFile in Directory.GetFiles(userFolder+avatarPath))
                    {
                        var configText = File.ReadAllText(avatarFile);
                        var tempConfig = JsonSerializer.Deserialize<AvatarConfigSpec>(configText);
                        if (tempConfig == null || tempConfig.id != avatarId)
                            continue;
                    
                        avatarConfig = tempConfig;
                        break;
                    }
                }
            }
            return avatarConfig;
        }
        
        public static void ParseNewAvatar(string newId)
        {


            AvatarConfigSpec avatarConfig = null;
            if (ChilloutVR.IsChilloutVRRunning())
            {
                avatarConfig = LoadConfigFrom(ChilloutVR.CCVROSCDirectory, "", newId);
            }
            else if (VRChat.IsVRChatRunning())
            {
                avatarConfig = LoadConfigFrom(VRChat.VRCOSCDirectory, "\\Avatars", newId);
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