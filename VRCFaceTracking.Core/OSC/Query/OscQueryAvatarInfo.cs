using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryAvatarInfo : IAvatarInfo
{
    public string Name { get; internal set; }

    public string Id { get; }

    public IParameterDefinition[] Parameters { get; }
    
    public OscQueryAvatarInfo(OSCQueryNode rootNode)
    {
        Name = "Half-baked OSCQuery impl";
        if (!rootNode.Contents.ContainsKey("change"))
        {
            // We likely queried while an avatar was still loading. Return without parsing.
            return;
        }
        Id = rootNode.Contents["change"].Value[0] as string;
        
        //TODO: Figure out a way to reconstruct the traditional address pattern instead of the whole thing.
        IEnumerable<IParameterDefinition> ConstructParameterArray(Dictionary<string, OSCQueryNode> entries)
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