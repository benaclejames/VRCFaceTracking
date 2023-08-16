using Newtonsoft.Json;

namespace VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;

public class OSCQueryNode
{
    // Empty Constructor for Json Serialization
    public OSCQueryNode(){}

    public OSCQueryNode(string fullPath)
    {
        FullPath = fullPath;
    }
        
    [JsonProperty("DESCRIPTION")]
    public string Description;

    [JsonProperty("FULL_PATH")] public string FullPath;

    [JsonProperty("ACCESS")]
    public AccessValues Access;

    [JsonProperty("CONTENTS")]
    public Dictionary<string, OSCQueryNode> Contents;

    [JsonProperty("TYPE")]
    public string OscType;

    [JsonProperty("VALUE")]
    public object[] Value;

    [JsonIgnore] 
    public string ParentPath {
        get
        {
            int length = Math.Max(1, FullPath.LastIndexOf("/"));
            return FullPath.Substring(0, length);
        }
            
    }

    [JsonIgnore]
    public string Name => FullPath.Substring(FullPath.LastIndexOf('/')+1);

    public override string ToString()
    {
        var result = JsonConvert.SerializeObject(this, WriteSettings);
        return result;
    }

    public static void AddConverter(JsonConverter c)
    {
        WriteSettings.Converters.Add(c);
    }

    private static JsonSerializerSettings WriteSettings = new JsonSerializerSettings()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
        
}