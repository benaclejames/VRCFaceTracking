﻿using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VRCFaceTracking
{
    public static class ConfigParser
    {
        private static string _oscDir;
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

        public static void ParseNewAvatar(string newId)
        {
            Logger.Msg("-------------------------------------------------");
            Logger.Msg("avatar id = " + newId);
            //check what mode we are in
            if(Globals.opMode == "vrc")
            {
              Logger.Msg("Loading Avatar from VRChat");
              _oscDir = VRChat.VRCOSCDirectory;
            }
            if (Globals.opMode == "cvr")
            {
              Logger.Msg("Loading Avatar from ChilloutVR");
              _oscDir = CVR.CVROSCDirectory;
            }
                    
            AvatarConfigSpec avatarConfig = null;
            foreach (var userFolder in Directory.GetDirectories(_oscDir))
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
            foreach (var parameter in UnifiedTrackingData.AllParameters)
                parameter.ResetParam(parameters);

            OnConfigLoaded();
        }
    }
}