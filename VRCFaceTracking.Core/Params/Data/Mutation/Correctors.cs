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

    [MutationProperty("MouthClosed/JawOpen Clamp")]
    public bool mouthClosedFix = true;
    [MutationProperty("LipSuck Limiter")]
    public bool lipSuckFix = true;

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
            data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight =
                data.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].Weight * (1f - data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight =
                data.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].Weight * (1f - data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight =
                data.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].Weight * (1f - data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight);
            data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight =
                data.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].Weight * (1f - data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight);
        }
    }
}
