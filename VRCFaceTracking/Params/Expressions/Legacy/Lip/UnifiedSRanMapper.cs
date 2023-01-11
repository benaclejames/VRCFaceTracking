using System;
using System.Collections.Generic;

namespace VRCFaceTracking.Params.Lip
{
    class UnifiedSRanMapper
    {
        private static Dictionary<SRanipal_LipShape_v2, Func<UnifiedTrackingData, float>> ShapeMap = new Dictionary<SRanipal_LipShape_v2, Func<UnifiedTrackingData, float>>
        {
            { SRanipal_LipShape_v2.JawRight, exp => exp.Shapes[(int)UnifiedExpressions.JawRight].AdjustedWeight },
            { SRanipal_LipShape_v2.JawLeft, exp => exp.Shapes[(int)UnifiedExpressions.JawLeft].AdjustedWeight },
            { SRanipal_LipShape_v2.JawForward, exp => exp.Shapes[(int)UnifiedExpressions.JawForward].AdjustedWeight },
            { SRanipal_LipShape_v2.JawOpen, exp => exp.Shapes[(int)UnifiedExpressions.JawOpen].AdjustedWeight - exp.Shapes[(int)UnifiedExpressions.MouthClosed].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthApeShape, exp => exp.Shapes[(int)UnifiedExpressions.MouthClosed].AdjustedWeight},

            { SRanipal_LipShape_v2.MouthUpperRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperRight].AdjustedWeight},
            { SRanipal_LipShape_v2.MouthUpperLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperLeft].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthLowerRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerRight].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthLowerLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerLeft].AdjustedWeight },

            { SRanipal_LipShape_v2.MouthUpperOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].AdjustedWeight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].AdjustedWeight) / 2.0f },

            { SRanipal_LipShape_v2.MouthPout, exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipPuckerRight].AdjustedWeight) / 2.0f },

            { SRanipal_LipShape_v2.MouthSmileRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthSmileLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthSadRight, exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].AdjustedWeight) },
            { SRanipal_LipShape_v2.MouthSadLeft, exp => Math.Min(1, exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].AdjustedWeight) },

            { SRanipal_LipShape_v2.CheekPuffLeft, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].AdjustedWeight },
            { SRanipal_LipShape_v2.CheekPuffRight, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].AdjustedWeight },
            { SRanipal_LipShape_v2.CheekSuck, exp => (exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].AdjustedWeight) / 2.0f },

            { SRanipal_LipShape_v2.MouthUpperUpRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthUpperUpLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthLowerDownRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].AdjustedWeight },
            { SRanipal_LipShape_v2.MouthLowerDownLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].AdjustedWeight },

            { SRanipal_LipShape_v2.MouthUpperInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckUpperLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipSuckUpperRight].AdjustedWeight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckLowerLeft].AdjustedWeight + exp.Shapes[(int)UnifiedExpressions.LipSuckLowerRight].AdjustedWeight) / 2.0f },

            { SRanipal_LipShape_v2.MouthLowerOverlay, exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserLower].AdjustedWeight},

            { SRanipal_LipShape_v2.TongueLongStep1, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].AdjustedWeight},
            { SRanipal_LipShape_v2.TongueLongStep2, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].AdjustedWeight},

            { SRanipal_LipShape_v2.TongueDown, exp => exp.Shapes[(int)UnifiedExpressions.TongueDown].AdjustedWeight},
            { SRanipal_LipShape_v2.TongueUp, exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].AdjustedWeight},
            { SRanipal_LipShape_v2.TongueRight, exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].AdjustedWeight},
            { SRanipal_LipShape_v2.TongueLeft, exp => exp.Shapes[(int)UnifiedExpressions.TongueLeft].AdjustedWeight},
            { SRanipal_LipShape_v2.TongueRoll, exp => exp.Shapes[(int)UnifiedExpressions.TongueRoll].AdjustedWeight},

            { SRanipal_LipShape_v2.TongueUpLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueUpRightMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownRightMorph, exp => 0.0f},

            { SRanipal_LipShape_v2.Max, exp => 0.0f},

        };

        public static float GetTransformedShape(SRanipal_LipShape_v2 sr_shape, UnifiedTrackingData expData) =>
            ShapeMap[sr_shape].Invoke(expData);
    }
}
