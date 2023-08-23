using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Babble;
public partial class BabbleOSC
{
    public TwoKeyDictionary<UnifiedExpressions, string, float> BabbleExpressionMap = new()
    {
        { UnifiedExpressions.JawOpen, "/cheekPuff", 0f },
        { UnifiedExpressions.NoseSneerLeft, "/noseSneerLeft", 0f },
        { UnifiedExpressions.NoseSneerRight, "/noseSneerRight", 0f },
        { UnifiedExpressions.JawOpen, "/jawOpen", 0f },
        { UnifiedExpressions.JawForward, "/jawForward", 0f },
        { UnifiedExpressions.JawLeft, "/jawLeft", 0f },
        { UnifiedExpressions.JawRight, "/jawRight", 0f },
        { UnifiedExpressions.LipFunnelLowerLeft, "/mouthFunnel", 0f },
        { UnifiedExpressions.LipFunnelLowerRight, "/mouthFunnel", 0f },
        { UnifiedExpressions.LipFunnelUpperLeft, "/mouthFunnel", 0f },
        { UnifiedExpressions.LipFunnelUpperRight, "/mouthFunnel", 0f },
        { UnifiedExpressions.LipPuckerLowerLeft, "/mouthPucker", 0f },
        { UnifiedExpressions.LipPuckerLowerRight, "/mouthPucker", 0f },
        { UnifiedExpressions.LipPuckerUpperLeft, "/mouthPucker", 0f },
        { UnifiedExpressions.LipPuckerUpperRight, "/mouthPucker", 0f },
        { UnifiedExpressions.MouthPressLeft, "/mouthLeft", 0f },
        { UnifiedExpressions.MouthPressRight, "/mouthRight", 0f },
        { UnifiedExpressions.LipSuckUpperLeft, "/mouthRollUpper", 0f },
        { UnifiedExpressions.LipSuckUpperRight, "/mouthRollUpper", 0f },
        { UnifiedExpressions.LipSuckLowerLeft, "/mouthRollLower", 0f },
        { UnifiedExpressions.LipSuckLowerRight, "/mouthRollLower", 0f },
        { UnifiedExpressions.MouthRaiserUpper, "/mouthShrugUpper", 0f },
        { UnifiedExpressions.MouthRaiserLower, "/mouthShrugLower", 0f },
        { UnifiedExpressions.MouthClosed, "/mouthClose", 0f },
        { UnifiedExpressions.MouthCornerPullLeft, "/mouthSmileLeft", 0f },
        { UnifiedExpressions.MouthCornerPullRight, "/mouthSmileRight", 0f },
        { UnifiedExpressions.MouthFrownLeft, "/mouthFrownLeft", 0f },
        { UnifiedExpressions.MouthFrownRight, "/mouthFrownRight", 0f },
        { UnifiedExpressions.MouthDimpleLeft, "/mouthDimpleLeft", 0f },
        { UnifiedExpressions.MouthDimpleRight, "/mouthDimpleRight", 0f },
        { UnifiedExpressions.MouthUpperUpLeft, "/mouthUpperUpLeft", 0f },
        { UnifiedExpressions.MouthUpperUpRight, "/mouthUpperUpRight", 0f },
        { UnifiedExpressions.MouthLowerDownLeft, "/mouthLowerDownLeft", 0f },
        { UnifiedExpressions.MouthLowerDownRight, "/mouthLowerDownRight", 0f },
        { UnifiedExpressions.MouthPressLeft, "/mouthPressLeft", 0f },
        { UnifiedExpressions.MouthPressRight, "/mouthPressRight", 0f },
        { UnifiedExpressions.MouthStretchLeft, "/mouthStretchLeft", 0f },
        { UnifiedExpressions.MouthStretchRight, "/mouthStretchRight", 0f },
        { UnifiedExpressions.TongueOut, "/tongueOut", 0f },
    };
}
