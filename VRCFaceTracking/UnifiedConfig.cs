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
        private static string unifiedConfigPath = Utils.PersistentDataDirectory + "\\Config.json";
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

        public List<string> RequestedModulePaths = new List<string>();
        public UnifiedTrackingData Data = new UnifiedTrackingData();
        public UnifiedTrackingMutator Mutator;

        public void ReadConfiguration()
        {
            UnifiedConfig config = new UnifiedConfig();
            try
            {
                string jsonString = File.ReadAllText(unifiedConfigPath);
                config = JsonSerializer.Deserialize<UnifiedConfig>(jsonString, jsonSerializerOptions);
                if (config != null)
                {
                    if (config.RequestedModulePaths != null && config.RequestedModulePaths.Count > 0)
                    {
                        Logger.Msg("Saved module load order initialized.");
                        UnifiedLibManager.RequestedModules = UnifiedLibManager.LoadExternalAssemblies(config.RequestedModulePaths.ToArray());
                    }
                    else Logger.Msg("Load order not found; initializing default order scheme.");

                    if (config.Mutator != null)
                    {
                        Logger.Msg("Saved data mutation settings loaded.");
                        UnifiedTracking.Mutator = config.Mutator;
                    }
                    else Logger.Msg("Mutators not found; initializing default mutator.");
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Warning("Configuration file not found; creating placeholder 'Config.json' file.");
                Save();
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

                // UnifiedTrackingData
                tempConfig.Data = UnifiedTracking.Data;

                // UnifiedTrackingMutator
                tempConfig.Mutator = UnifiedTracking.Mutator;

                // Assembly load order.
                tempConfig.RequestedModulePaths = new List<string>();

                UnifiedLibManager.RequestedModules.ForEach(a => tempConfig.RequestedModulePaths.Add(a.Location));

                JsonSerializer.Serialize(stream, tempConfig, jsonSerializerOptions);
                stream.Dispose();
            }
        }
    }
}
