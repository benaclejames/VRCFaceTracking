using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryAvatarInfo : IAvatarInfo
{
    public string Name { get; internal set; }

    public string Id { get; }

    public IParameterDefinition[] Parameters { get; }
    
    public OscQueryAvatarInfo(OscQueryNode rootNode)
    {
        if (!rootNode.Contents.ContainsKey("parameters"))
        {
            return;
        }
        
        Name = "Unknown";
        Id = rootNode.Contents.TryGetValue("change", out var change) ? change.Value[0] as string : "Unknown";
        
        //TODO: Figure out a way to reconstruct the traditional address pattern instead of the whole thing.
        IEnumerable<IParameterDefinition> ConstructParameterArray(Dictionary<string, OscQueryNode> entries)
        {
            return entries
                .SelectMany(entry =>
                    entry.Value.Contents != null ? ConstructParameterArray(entry.Value.Contents) : !string.IsNullOrEmpty(entry.Value.OscType) ? new[] { new OscQueryParameterDef(entry.Value.FullPath, entry.Value) } : Array.Empty<OscQueryParameterDef>()
                );
        }
        
        Parameters = rootNode.Contents["parameters"].Contents
            .SelectMany(entry => 
                entry.Value.Contents != null ? ConstructParameterArray(entry.Value.Contents) : !string.IsNullOrEmpty(entry.Value.OscType) ? new[] { new OscQueryParameterDef(entry.Value.FullPath, entry.Value) } : Array.Empty<OscQueryParameterDef>()
            )
            .ToArray();
    }
}