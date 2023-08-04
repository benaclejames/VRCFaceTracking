using System.Linq;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Mutators;

namespace VRCFaceTracking.Core.Models;

public struct UnifiedMutationInfo
{
    public string MutationName { get; set; }
    public UnifiedMutationProperty[] Properties { get; set; }
}

public struct UnifiedMutationConfig
{
    public List<UnifiedMutationInfo> MutationInfo = new();

    public UnifiedMutationConfig()
    {
    }
}