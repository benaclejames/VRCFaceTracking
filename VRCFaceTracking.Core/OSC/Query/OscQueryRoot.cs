namespace VRCFaceTracking.Core.OSC.Query;

public class OscQueryRoot : OscQueryNode
{
    // Acts as a quick lookup for full addresses
    private readonly Dictionary<string, OscQueryNode> _nodes;

    public OscQueryRoot() : base("/")
    {
        _nodes = new Dictionary<string, OscQueryNode>
        {
            { "/", this }
        };
    }

    public OscQueryNode AddNode(OscQueryNode node)
    {
        // First, we check if the parent node exists in _nodes. If it doesn't, call this function with that node and recurse
        // if it does, add this node to the contents of that node, as well as _nodes
        if (!_nodes.TryGetValue(node.ParentPath, out var parentNode))
        {
            parentNode = AddNode(new OscQueryNode(node.ParentPath));
        }
        parentNode.Contents ??= new Dictionary<string, OscQueryNode>();
        parentNode.Contents.Add(node.Name, node);
        _nodes.Add(node.FullPath, node);

        return node;
    }
}