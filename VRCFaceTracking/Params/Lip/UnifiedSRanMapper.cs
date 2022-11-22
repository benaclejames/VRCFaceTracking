using System;
using System.Collections.Generic;

namespace VRCFaceTracking.Params.Lip
{
    class UnifiedSRanMapper
    {
        private static Dictionary<SRanipal_LipShape_v2, Func<UnifiedExpressionsData, float?>> ShapeMap = new Dictionary<SRanipal_LipShape_v2, Func<UnifiedExpressionsData, float?>>
        {
            { SRanipal_LipShape_v2.JawRight, exp => exp.Shapes[(int)UnifiedExpressions.JawRight].weight },
            { SRanipal_LipShape_v2.JawLeft, exp => exp.Shapes[(int)UnifiedExpressions.JawOpen].weight },
            { SRanipal_LipShape_v2.JawForward, exp => exp.Shapes[(int)UnifiedExpressions.JawForward].weight },
            { SRanipal_LipShape_v2.JawOpen, exp => exp.Shapes[(int)UnifiedExpressions.JawOpen].weight },
            { SRanipal_LipShape_v2.MouthApeShape, exp => exp.Shapes[(int)UnifiedExpressions.MouthApeShape].weight },

            { SRanipal_LipShape_v2.MouthUpperRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthTopRight].weight},
            { SRanipal_LipShape_v2.MouthUpperLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthTopLeft].weight },
            { SRanipal_LipShape_v2.MouthLowerRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthBottomRight].weight },
            { SRanipal_LipShape_v2.MouthLowerLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthBottomLeft].weight },

            { SRanipal_LipShape_v2.MouthUpperOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopRight].weight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight].weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthPout, exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerRight].weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthSmileRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].weight },
            { SRanipal_LipShape_v2.MouthSmileLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].weight },
            { SRanipal_LipShape_v2.MouthSadRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].weight },
            { SRanipal_LipShape_v2.MouthSadLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].weight },

            { SRanipal_LipShape_v2.CheekPuffLeft, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].weight },
            { SRanipal_LipShape_v2.CheekPuffRight, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].weight },
            { SRanipal_LipShape_v2.CheekSuck, exp => (exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthUpperUpRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopRight].weight },
            { SRanipal_LipShape_v2.MouthUpperUpLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft].weight },
            { SRanipal_LipShape_v2.MouthLowerDownRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight].weight },
            { SRanipal_LipShape_v2.MouthLowerDownLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft].weight },

            { SRanipal_LipShape_v2.MouthUpperInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckTopLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipSuckTopRight].weight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckBottomLeft].weight + exp.Shapes[(int)UnifiedExpressions.LipSuckBottomRight].weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthLowerOverlay, exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserLower].weight},

            { SRanipal_LipShape_v2.TongueLongStep1, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].weight / 2.0f},
            { SRanipal_LipShape_v2.TongueLongStep2, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].weight / 2.0f},

            { SRanipal_LipShape_v2.TongueDown, exp => exp.Shapes[(int)UnifiedExpressions.TongueDown].weight},
            { SRanipal_LipShape_v2.TongueUp, exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].weight},
            { SRanipal_LipShape_v2.TongueRight, exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].weight},
            { SRanipal_LipShape_v2.TongueLeft, exp => exp.Shapes[(int)UnifiedExpressions.TongueLeft].weight},
            { SRanipal_LipShape_v2.TongueRoll, exp => exp.Shapes[(int)UnifiedExpressions.TongueRoll].weight},

            { SRanipal_LipShape_v2.TongueUpLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueUpRightMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownRightMorph, exp => 0.0f},

            { SRanipal_LipShape_v2.Max, exp => 0.0f},

        };

        public static float GetSRanipalShapeFromUnifiedShapes(SRanipal_LipShape_v2 sr_shape, UnifiedExpressionsData expData) =>
            (float)ShapeMap[sr_shape].Invoke(expData);
    }
}
