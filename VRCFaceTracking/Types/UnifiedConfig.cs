using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.Params;

namespace VRCFaceTracking.Types
{
    public class UnifiedConfig
    {
        public List<string> RequestedModulePaths;
        public UnifiedTrackingData Data;
        private static string unifiedConfigPath = Utils.PersistentDataDirectory + "\\Config.json";

        public void ReadConfiguration()
        {
            UnifiedConfig config = new UnifiedConfig();

            UnifiedLibManager.ReloadModules();

            try
            {
                string jsonString = File.ReadAllText(unifiedConfigPath);
                config = JsonSerializer.Deserialize<UnifiedConfig>(jsonString, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
                if (config != null)
                {
                    if (config.RequestedModulePaths.Count > 0)
                    {
                        Logger.Msg("Saved module load order initialized.");
                        UnifiedLibManager._requestedModules = UnifiedLibManager.LoadExternalAssemblies(config.RequestedModulePaths.ToArray());
                    }
                    else
                    {
                        Logger.Msg("Load order not found; initializing default order scheme.");
                    }

                    if (config.Data != null)
                    {
                        UnifiedTracking.Data = config.Data;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Warning("Configuration file not found, will be created on proper exit of application.");
            }
            catch (JsonException)
            {
                Logger.Warning("Configuration file has been corrupted, purging file.");
                File.Delete(unifiedConfigPath);
            }
        }

        public void Save()
        {
            using (Stream stream = File.Open(unifiedConfigPath, FileMode.Create))
            {
                UnifiedConfig tempConfig = new UnifiedConfig();

                tempConfig.Data = UnifiedTracking.Data;
                tempConfig.RequestedModulePaths = new List<string>();
                UnifiedLibManager._requestedModules.ForEach(a => tempConfig.RequestedModulePaths.Add(a.Location));

                JsonSerializer.Serialize(stream, tempConfig, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
                stream.Dispose();
            }
        }
    }
}
