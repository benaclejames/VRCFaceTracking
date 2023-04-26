using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Types
{
    public class UnifiedConfig
    {
        private static string unifiedConfigPath = Utils.PersistentDataDirectory + "\\Config.json";
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

        public UnifiedTrackingData Data = new UnifiedTrackingData();
        public UnifiedTrackingMutator Mutator;

        public void ReadConfiguration()
        {
            var config = new UnifiedConfig();
            try
            {
                string jsonString = File.ReadAllText(unifiedConfigPath);
                config = JsonSerializer.Deserialize<UnifiedConfig>(jsonString, jsonSerializerOptions);
                if (config != null)
                {
                    if (config.Mutator != null)
                        UnifiedTracking.Mutator = config.Mutator;
                }
            }
            catch (FileNotFoundException)
            {
                Save();
            }
            catch (JsonException)
            {
                File.Delete(unifiedConfigPath);
            }
        }

        public static void Save()
        {
            using Stream stream = File.Open(unifiedConfigPath, FileMode.Create);
            
            var tempConfig = new UnifiedConfig
            {
                // UnifiedTrackingData
                Data = UnifiedTracking.Data,
                // UnifiedTrackingMutator
                Mutator = UnifiedTracking.Mutator
            };

            JsonSerializer.Serialize(stream, tempConfig, jsonSerializerOptions);
            stream.Dispose();
        }
    }
}
