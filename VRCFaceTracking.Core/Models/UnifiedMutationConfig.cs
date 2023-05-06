using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Models;

public struct UnifiedMutationConfig
{
    public UnifiedMutation[] ShapeMutations;
    public UnifiedMutation GazeMutations, OpennessMutations, PupilMutations;
}