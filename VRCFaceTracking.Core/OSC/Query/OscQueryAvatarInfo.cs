using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Core.OSC.Query.mDNS.Types.OscQuery;

public class OscQueryAvatarInfo : IAvatarInfo
{
    public string Name => "Half-baked OSCQuery impl";

    public string Id { get; }

    public IParameterDefinition[] Parameters { get; }
    
    public OscQueryAvatarInfo(OSCQueryNode rootNode)
    {
        Id = rootNode.Contents["change"].Value[0] as string;
        
        Parameters = rootNode.Contents["parameters"].Contents.Where(p => (int)p.Value.Access > 1)
            .Select(entry => new OscQueryParameterDef(entry.Key, entry.Value)).ToArray<IParameterDefinition>();
    }
}