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

    public override void MutateData(ref UnifiedTrackingData data)
    {
        data.Shapes[(int)UnifiedExpressions.MouthClosed].Weight = Math.Min(
            data.Shapes[(int)UnifiedExpressions.MouthClosed].Weight,
            data.Shapes[(int)UnifiedExpressions.JawOpen].Weight);
    }
}
