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
    

    private float BlendParam(float currentValue, float influencerValue) => Math.Clamp(currentValue * (1.0f - eyeLidBlend * 0.5f) + influencerValue * (eyeLidBlend * 0.5f), 0.0f, 1.0f);

    private void BlendOpposingParams(ref float leftParam, ref float rightParam)
    {
        leftParam = BlendParam(leftParam, rightParam);
        rightParam = BlendParam(rightParam, leftParam);
    }
    
    private void BlendUnifiedExpressionParams(ref UnifiedTrackingData data, UnifiedExpressions leftExpression, UnifiedExpressions rightExpression) => BlendOpposingParams(ref data.Shapes[(int)leftExpression].Weight, ref data.Shapes[(int)rightExpression].Weight);
    
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
            BlendOpposingParams(ref data.Eye.Left.Openness, ref data.Eye.Right.Openness);
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.EyeWideLeft, UnifiedExpressions.EyeWideRight);
            
            // Thx to Hash for suggesting the rest of these
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.EyeSquintLeft, UnifiedExpressions.EyeSquintRight);
            BlendOpposingParams(ref data.Eye.Left.PupilDiameter_MM, ref data.Eye.Right.PupilDiameter_MM);
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.BrowPinchLeft, UnifiedExpressions.BrowPinchRight);
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.BrowLowererLeft, UnifiedExpressions.BrowLowererRight);
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.BrowInnerUpLeft, UnifiedExpressions.BrowInnerUpRight);
            BlendUnifiedExpressionParams(ref data, UnifiedExpressions.BrowOuterUpLeft, UnifiedExpressions.BrowOuterUpRight);
        }
    }
}
