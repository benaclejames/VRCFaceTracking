using Newtonsoft.Json;

namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryNode
{
    // Empty Constructor for Json Serialization
    public OscQueryNode(){}

    public OscQueryNode(string fullPath, AccessValues access = AccessValues.NoValue, string oscType = null)
    {
        FullPath = fullPath;
        Access = access;
        OscType = oscType;
    }
        
    [JsonProperty("DESCRIPTION")]
    public string Description;

    [JsonProperty("FULL_PATH")] public string FullPath;

    [JsonProperty("ACCESS")]
    public AccessValues Access;

    [JsonProperty("CONTENTS")]
    public Dictionary<string, OscQueryNode> Contents;

    [JsonProperty("TYPE")]
    public string OscType;

    [JsonProperty("VALUE")]
    public object[] Value;

    [JsonIgnore] 
    public string ParentPath {
        get
        {
            var length = Math.Max(1, FullPath.LastIndexOf("/", StringComparison.Ordinal));
            return FullPath.Substring(0, length);
        }
            
    }

    [JsonIgnore]
    public string Name => FullPath.Substring(FullPath.LastIndexOf('/')+1);

    public override string ToString()
    {
        var result = JsonConvert.SerializeObject(this,  new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });
        return result;
    }
}