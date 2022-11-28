using System;
using System.Collections.Generic;

namespace VRCFaceTracking.Params.Lip
{
    class UnifiedSRanMapper
    {
        private static Dictionary<SRanipal_LipShape_v2, Func<UnifiedExpressionsData, float>> ShapeMap = new Dictionary<SRanipal_LipShape_v2, Func<UnifiedExpressionsData, float>>
        {
            { SRanipal_LipShape_v2.JawRight, exp => exp.Shapes[(int)UnifiedExpressions.JawRight].Weight },
            { SRanipal_LipShape_v2.JawLeft, exp => exp.Shapes[(int)UnifiedExpressions.JawLeft].Weight },
            { SRanipal_LipShape_v2.JawForward, exp => exp.Shapes[(int)UnifiedExpressions.JawForward].Weight },
            { SRanipal_LipShape_v2.JawOpen, exp => exp.Shapes[(int)UnifiedExpressions.JawOpen].Weight - exp.Shapes[(int)UnifiedExpressions.MouthClosed].Weight },
            { SRanipal_LipShape_v2.MouthApeShape, exp => exp.Shapes[(int)UnifiedExpressions.MouthClosed].Weight},

            { SRanipal_LipShape_v2.MouthUpperRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthTopRight].Weight},
            { SRanipal_LipShape_v2.MouthUpperLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthTopLeft].Weight },
            { SRanipal_LipShape_v2.MouthLowerRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthBottomRight].Weight },
            { SRanipal_LipShape_v2.MouthLowerLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthBottomLeft].Weight },

            { SRanipal_LipShape_v2.MouthUpperOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopRight].Weight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerOverturn, exp => (exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight].Weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthPout, exp => (exp.Shapes[(int)UnifiedExpressions.LipPuckerLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipPuckerRight].Weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthSmileRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileRight].Weight },
            { SRanipal_LipShape_v2.MouthSmileLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthSmileLeft].Weight },
            { SRanipal_LipShape_v2.MouthSadRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight },
            { SRanipal_LipShape_v2.MouthSadLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight },

            { SRanipal_LipShape_v2.CheekPuffLeft, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight },
            { SRanipal_LipShape_v2.CheekPuffRight, exp => exp.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight },
            { SRanipal_LipShape_v2.CheekSuck, exp => (exp.Shapes[(int)UnifiedExpressions.CheekSuckLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSuckRight].Weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthUpperUpRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopRight].Weight },
            { SRanipal_LipShape_v2.MouthUpperUpLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft].Weight },
            { SRanipal_LipShape_v2.MouthLowerDownRight, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight].Weight },
            { SRanipal_LipShape_v2.MouthLowerDownLeft, exp => exp.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft].Weight },

            { SRanipal_LipShape_v2.MouthUpperInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckTopLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckTopRight].Weight) / 2.0f },
            { SRanipal_LipShape_v2.MouthLowerInside, exp => (exp.Shapes[(int)UnifiedExpressions.LipSuckBottomLeft].Weight + exp.Shapes[(int)UnifiedExpressions.LipSuckBottomRight].Weight) / 2.0f },

            { SRanipal_LipShape_v2.MouthLowerOverlay, exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserLower].Weight},

            { SRanipal_LipShape_v2.TongueLongStep1, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].Weight},
            { SRanipal_LipShape_v2.TongueLongStep2, exp => exp.Shapes[(int)UnifiedExpressions.TongueOut].Weight},

            { SRanipal_LipShape_v2.TongueDown, exp => exp.Shapes[(int)UnifiedExpressions.TongueDown].Weight},
            { SRanipal_LipShape_v2.TongueUp, exp => exp.Shapes[(int)UnifiedExpressions.TongueUp].Weight},
            { SRanipal_LipShape_v2.TongueRight, exp => exp.Shapes[(int)UnifiedExpressions.TongueRight].Weight},
            { SRanipal_LipShape_v2.TongueLeft, exp => exp.Shapes[(int)UnifiedExpressions.TongueLeft].Weight},
            { SRanipal_LipShape_v2.TongueRoll, exp => exp.Shapes[(int)UnifiedExpressions.TongueRoll].Weight},

            { SRanipal_LipShape_v2.TongueUpLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueUpRightMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownLeftMorph, exp => 0.0f},
            { SRanipal_LipShape_v2.TongueDownRightMorph, exp => 0.0f},

            { SRanipal_LipShape_v2.Max, exp => 0.0f},

        };

        public static float GetSRanipalShapeFromUnifiedShapes(SRanipal_LipShape_v2 sr_shape, UnifiedExpressionsData expData) =>
            ShapeMap[sr_shape].Invoke(expData);
    }
}
