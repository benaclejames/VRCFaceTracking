using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;

public class OscQueryAvatarInfo : IAvatarInfo
{
    public string Name => "Half-baked OSCQuery impl";

    public string Id { get; }

    public IParameterDefinition[] Parameters { get; }
    
    public OscQueryAvatarInfo(OSCQueryNode rootNode)
    {
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
                    entry.Value.Contents != null ? ConstructParameterArray(entry.Value.Contents) : new[] { new OscQueryParameterDef(entry.Value.FullPath, entry.Value) }
                );
        }
        
        Parameters = rootNode.Contents["parameters"].Contents
            .SelectMany(entry => 
                entry.Value.Contents != null ? ConstructParameterArray(entry.Value.Contents) : new[] { new OscQueryParameterDef(entry.Value.FullPath, entry.Value) }
            )
            .ToArray();
    }
}