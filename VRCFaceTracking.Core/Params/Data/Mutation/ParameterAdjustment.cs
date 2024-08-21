using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public class ParameterAdjustment : TrackingMutation
{
    [MutationButton("Reset Values")]
    public void Reset()
    {
        test.Item1 = 0f;
        test.Item2 = 1f;
        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            ranges[i].Item1 = 0f;
            ranges[i].Item2 = 1f;
        }
    }

    [MutationProperty("JawOpen", 0f, 1f)]
    public (float, float) test;
    [MutationProperty(typeof(UnifiedExpressions), 0f, 1f)]
    public (float, float)[] ranges = new (float, float)[(int)UnifiedExpressions.Max];
    public override string Name => "Parameter Adjustment";

    public override string Description => "Adjust VRCFaceTracking Parameters.";

    public override MutationPriority Step => MutationPriority.None;

    public override void MutateData(ref UnifiedTrackingData data)
    {
        Logger.LogInformation($"Test Range 1: {test.Item1} 2: {test.Item2}");
        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            //Logger.LogInformation($"{(UnifiedExpressions)i} Range 1: {ranges[i].Item1} 2: {ranges[i].Item2}");
            data.Shapes[i].Weight = (data.Shapes[i].Weight - ranges[i].Item1) / (ranges[i].Item2 - ranges[i].Item1);
        }
    }
}
