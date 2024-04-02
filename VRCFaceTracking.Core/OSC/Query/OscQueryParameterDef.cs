using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryParameterDef : IParameterDefinition
{
    public string Address { get; }
    public string Name { get; }
    public Type Type { get; }

    public OscQueryParameterDef(string address, OscQueryNode node)
    {
        Address = address;
        Name = node.Name;
        Type = OscUtils.TypeConversions.First(t =>
                t.Key.typeChar
                    .Contains(node.OscType
                        .First()))
            .Key
            .Item1;
    }
}