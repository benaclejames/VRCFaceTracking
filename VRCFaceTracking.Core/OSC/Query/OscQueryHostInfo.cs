using Newtonsoft.Json;

namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryHostInfo
{
    [JsonProperty("NAME")]
    public string name;

    [JsonProperty("EXTENSIONS")] public Dictionary<string, bool> extensions = new()
    {
        { "ACCESS", true },
        { "CLIPMODE", false },
        { "RANGE", true },
        { "TYPE", true },
        { "VALUE", true },
    };
        
    [JsonProperty("OSC_IP")]
    public string oscIP;
        
    [JsonProperty("OSC_PORT")]
    public int oscPort = 6969;

    [JsonProperty("OSC_TRANSPORT")] 
    public string oscTransport = "UDP";
    
    /// <summary>
    /// Empty Constructor required for JSON Serialization
    /// </summary>
    public OscQueryHostInfo()
    {
            
    }

    public override string ToString()
    {
        var result = JsonConvert.SerializeObject(this);
        return result;
    }
}