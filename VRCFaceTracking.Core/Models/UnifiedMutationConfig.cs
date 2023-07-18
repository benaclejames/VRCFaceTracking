using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Models;

public struct UnifiedMutationConfig
{
    public UnifiedMutation[] ShapeMutations;
    public UnifiedMutation GazeMutations, OpennessMutations, PupilMutations;

    public UnifiedMutationConfig()
    {
        ShapeMutations = new UnifiedMutation[(int)UnifiedExpressions.Max + 1];
        GazeMutations = default;
        OpennessMutations = default;
        PupilMutations = default;
    }
}