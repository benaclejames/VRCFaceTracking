using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Models;

public struct UnifiedMutationConfig
{
    public MutationConfig[] ShapeMutations;
    public MutationConfig GazeMutationsConfig, OpennessMutationsConfig, PupilMutationsConfig;

    public UnifiedMutationConfig()
    {
        ShapeMutations = new MutationConfig[(int)UnifiedExpressions.Max + 1];
        for (int i = 0; i < ShapeMutations.Length; i++)
        {
            ShapeMutations[i] = new MutationConfig()
            {
                Name = ((UnifiedExpressions)i).ToString()
            };
        }
        GazeMutationsConfig = new MutationConfig()
        {
            Name = "GazeMutations"
        };
        OpennessMutationsConfig = new MutationConfig()
        {
            Name = "OpennessMutations"
        };
        PupilMutationsConfig = new MutationConfig()
        {
            Name = "PupilMutations"
        };
    }
}