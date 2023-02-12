using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Params
{
    /// <summary> 
    /// Represents the type of transformed and simplified Shape data that is converted from the base UnifiedExpression data in the form of enumerated shapes.
    /// </summary>
    /// <remarks>
    /// These shapes have a basis on general common expressions from different tracking interfaces and standards 
    /// and are directly transformed from Unified Expressions data.
    /// </remarks>
    public enum UnifiedSimpleExpressions
    {
        BrowDownRight, // Lowers and pinches the right eyebrow.
        BrowDownLeft, // Lowers and pinches the left eyebrow.
        MouthSmileRight, // Moves the right corner of the lip into a smile expression.
        MouthSmileLeft, // Moves the left corner of the lip into a smile expression.
        MouthSadRight, // Moves the right corner of the lip into a sad expression.
        MouthSadLeft, // Moves the left corner of the lip into a sad expression.
        MouthUpperUpRight, // Raises the entire upper right of the lips.
        MouthUpperUpLeft, // Raises the entire upper left of the lips.
    }

    public static class UnifiedSimplifier
    {
        /// <summary>
        /// Data map of all Unified to Unified Simple expressions. 
        /// </summary>
        /// <remarks>This is to keep all conversion data self contained and easily accessible.</remarks>
        public static Dictionary<UnifiedSimpleExpressions, Func<UnifiedTrackingData, float>> ExpressionMap = new Dictionary<UnifiedSimpleExpressions, Func<UnifiedTrackingData, float>>
        {
            { UnifiedSimpleExpressions.BrowDownRight, exp =>
                exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight * .75f + exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight * .25f },
            { UnifiedSimpleExpressions.BrowDownLeft, exp =>
                exp.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight * .75f + exp.Shapes[(int)UnifiedExpressions.BrowPinchLeft].Weight * .25f },

            { UnifiedSimpleExpressions.MouthSmileRight, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight * .8f + exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantRight].Weight * .2f },
            { UnifiedSimpleExpressions.MouthSmileLeft, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight * .8f + exp.Shapes[(int)UnifiedExpressions.MouthCornerSlantLeft].Weight * .2f },
            { UnifiedSimpleExpressions.MouthSadRight, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight > exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight ?
                exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight : exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight },
            { UnifiedSimpleExpressions.MouthSadLeft, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight > exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight ?
                exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight : exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight },

            { UnifiedSimpleExpressions.MouthUpperUpRight, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthUpperInnerUpRight].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthUpperDeepenRight].Weight * .5f },
            { UnifiedSimpleExpressions.MouthUpperUpLeft, exp =>
                exp.Shapes[(int)UnifiedExpressions.MouthUpperInnerUpLeft].Weight * .5f + exp.Shapes[(int)UnifiedExpressions.MouthUpperDeepenLeft].Weight * .5f },
        };
    }
}
