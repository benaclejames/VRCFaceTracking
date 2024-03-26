using Newtonsoft.Json;

namespace VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;

public class OscQueryHostInfo
{
    [JsonProperty(Keys.NAME)]
    public string name;

    [JsonProperty(Keys.EXTENSIONS)] public Dictionary<string, bool> extensions = new Dictionary<string, bool>()
    {
        { "ACCESS", true },
        { "CLIPMODE", false },
        { "RANGE", true },
        { "TYPE", true },
        { "VALUE", true },
    };
        
    [JsonProperty(Keys.OSC_IP)]
    public string oscIP;
        
    [JsonProperty(Keys.OSC_PORT)]
    public int oscPort = 6969;

    [JsonProperty(Keys.OSC_TRANSPORT)] 
    public string oscTransport = Keys.OSC_TRANSPORT_UDP;

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

    public class Keys
    {
        public const string NAME = "NAME";
        public const string EXTENSIONS = "EXTENSIONS";
        public const string OSC_IP = "OSC_IP";
        public const string OSC_PORT = "OSC_PORT";
        public const string OSC_TRANSPORT = "OSC_TRANSPORT";
        public const string OSC_TRANSPORT_UDP = "UDP";
    }
}