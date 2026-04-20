using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public class Correctors : TrackingMutation
{
    public override string Name => "Unified Correctors";
    public override string Description => "Processes data to conform to Unified Expressions.";
    public override MutationPriority Step => MutationPriority.Postprocessor;
    public override bool IsActive { get; set; } = true;

    [MutationProperty("MouthClosed/JawOpen Clamp", true)]
    public bool mouthClosedFix = true;
    [MutationProperty("LipSuck Limiter", true)]
    public bool lipSuckFix = true;
    
    // How much "influence" the opposite eye has on openness
    [MutationProperty("EyeLid Blend", true)]
    public float eyeLidBlend = 0.0f;    // 0.00f meaning zero influence, 1.00f meaning the value of each eyelid is the median of both
    

    public override void MutateData(ref UnifiedTrackingData data)
    {
        if (mouthClosedFix)
        {
            data.Shapes[(int)UnifiedExpressions.MouthClosed].Weight =
                Math.Min(
                    data.Shapes[(int)UnifiedExpressions.MouthClosed].Weight,
                    data.Shapes[(int)UnifiedExpressions.JawOpen].Weight
                );
        }

        if (lipSuckFix)
        {
            data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight *= (1f - data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight *= (1f - data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight *= (1f - data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight *= (1f - data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight);
        }

        if (eyeLidBlend > 0.00f)
        {
            // Half the blend because we don't want users swapping eyes unintentionally
            var blend = eyeLidBlend * 0.5f;
            var inverseBlend = 1.0f - blend;
            
            // Pre-calculate what our parameters would end up using internally
            var leftCalculated  = data.Eye.Left.Openness  * 0.75f + data.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight  * 0.25f;
            var rightCalculated = data.Eye.Right.Openness * 0.75f + data.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * 0.25f;

            // Actual blending logic
            var leftResultant  = leftCalculated * inverseBlend + rightCalculated * blend;
            var rightResultant = rightCalculated * inverseBlend + leftCalculated * blend;

            // Solve for our original values
            data.Eye.Left.Openness = Math.Clamp(leftResultant, 0.0f, 1.0f);
            data.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight = Math.Clamp(leftResultant, 0.0f, 1.0f);
            data.Eye.Right.Openness = Math.Clamp(rightResultant, 0.0f, 1.0f);
            data.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight = Math.Clamp(rightResultant, 0.0f, 1.0f);
        }
    }
}
