using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Models;

public struct UnifiedMutationConfig
{
    public UnifiedMutation[] ShapeMutations;
    public UnifiedMutation GazeMutations, OpennessMutations, PupilMutations;

    public UnifiedMutationConfig()
    {
        ShapeMutations = new UnifiedMutation[(int)UnifiedExpressions.Max + 1];
        for (int i = 0; i < ShapeMutations.Length; i++)
        {
            ShapeMutations[i] = new UnifiedMutation()
            {
                Name = ((UnifiedExpressions)i).ToString()
            };
        }
        GazeMutations = new UnifiedMutation()
        {
            Name = "GazeMutations"
        };
        OpennessMutations = new UnifiedMutation()
        {
            Name = "OpennessMutations"
        };
        PupilMutations = new UnifiedMutation()
        {
            Name = "PupilMutations"
        };
    }
}