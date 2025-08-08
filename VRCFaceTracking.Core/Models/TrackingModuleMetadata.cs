using System.Text.Json.Serialization;

namespace VRCFaceTracking.Core.Models;

public class TrackingModuleMetadata
{
    public Guid ModuleId
    {
        get; set;
    }

    public DateTime LastUpdated
    {
        get; set;
    } = DateTime.Now;

    public string Version
    {
        get;
        set;
    } = "Unknown";
    
    public int Downloads
    {
        get; set;
    }
    
    public int Ratings
    {
        get; set;
    }
    
    public float Rating
    {
        get; set;
    }

    public string AuthorName
    {
        get; set;
    } = "(No author provided)";

    public string ModuleName
    {
        get; set;
    } = "(No name provided)";
    
    public string ModuleDescription
    {
        get; set;
    } = "(No description provided)";
    
    public string UsageInstructions
    {
        get; set;
    } = "(No usage instructions provided. Check the module page for more information.)";

    public string DownloadUrl
    {
        get; set;
    } = "(No download provided)";

    public string ModulePageUrl
    {
        get;
        set;
    }
    
    public string DllFileName
    {
        get; set;
    } = "(No DLL provided)";
    
    public string FileHash
    {
        get; set;
    }
}
