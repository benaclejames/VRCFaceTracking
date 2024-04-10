using Newtonsoft.Json;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.Models.Osc.FileBased;

public class AvatarConfigFileIODef
{
    public string address { get; set; }
    public string type { get; set; }

    [JsonIgnore]
    public Type Type => OscUtils.TypeConversions.Where(conversion => conversion.Value.configType == type).Select(conversion => conversion.Key).FirstOrDefault().Item1;

}