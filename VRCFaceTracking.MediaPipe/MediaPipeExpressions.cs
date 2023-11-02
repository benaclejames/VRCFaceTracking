using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.MediaPipe;

public partial class MediaPipeOSC
{
    public readonly TwoKeyDictionary<string, HashSet<UnifiedExpressions>, float> Expressions = new()
    {
        { "/mouthPucker", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.LipPuckerLowerLeft,
            UnifiedExpressions.LipPuckerLowerRight,
            UnifiedExpressions.LipPuckerUpperLeft,
            UnifiedExpressions.LipPuckerUpperRight }, 0f } ,
        { "/mouthFunnel", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.LipFunnelLowerLeft,
            UnifiedExpressions.LipFunnelLowerRight,
            UnifiedExpressions.LipFunnelUpperLeft,
            UnifiedExpressions.LipFunnelUpperRight }, 0f },
        { "/browInnerUp", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.BrowInnerUpLeft,
            UnifiedExpressions.BrowInnerUpLeft }, 0f },
        { "/cheekPuff", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.CheekPuffLeft,
            UnifiedExpressions.CheekPuffRight }, 0f },
        { "/mouthLeft", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthUpperLeft,
            UnifiedExpressions.MouthLowerLeft }, 0f },
        { "/mouthRight", new HashSet<UnifiedExpressions>() {
            UnifiedExpressions.MouthUpperRight,
            UnifiedExpressions.MouthLowerRight }, 0f },
        { "/browDownLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.BrowLowererLeft }, 0f },
        { "/browDownRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.BrowLowererRight }, 0f },
        { "/browOuterUpLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.BrowOuterUpLeft }, 0f},
        { "/browOuterUpRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.BrowOuterUpRight }, 0f },
        { "/cheekSquintLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.CheekSquintLeft}, 0f },
        { "/cheekSquintRight",new HashSet<UnifiedExpressions>() { UnifiedExpressions.CheekSquintRight}, 0f },
        { "/eyeSquintLeft",new HashSet<UnifiedExpressions>() { UnifiedExpressions.EyeSquintLeft }, 0f },
        { "/eyeSquintRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.EyeSquintRight }, 0f },
        { "/eyeWideLeft" ,new HashSet<UnifiedExpressions>() { UnifiedExpressions.EyeWideLeft }, 0f},
        { "/eyeWideRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.EyeWideRight }, 0f },
        { "/jawForward", new HashSet<UnifiedExpressions>() { UnifiedExpressions.JawForward }, 0f },
        { "/jawLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.JawLeft }, 0f },
        { "/jawOpen", new HashSet<UnifiedExpressions>() { UnifiedExpressions.JawOpen }, 0f},
        { "/jawRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.JawRight }, 0f },
        { "/mouthClose", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthClosed }, 0f },
        { "/mouthDimpleLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthDimpleLeft }, 0f },
        { "/mouthDimpleRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthDimpleRight }, 0f},
        { "/mouthFrownLeft",new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthFrownLeft }, 0f },
        { "/mouthFrownRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthFrownRight }, 0f },
        { "/mouthLowerDownLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthLowerDownLeft }, 0f },
        { "/mouthLowerDownRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthLowerDownRight }, 0f },
        { "/mouthPressLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthPressLeft }, 0f },
        { "/mouthPressRight",new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthPressRight }, 0f },
        { "/mouthShrugLower", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthRaiserLower }, 0f },
        { "/mouthShrugUpper", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthRaiserUpper }, 0f },
        { "/mouthSmileLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthCornerPullLeft}, 0f },
        { "/mouthSmileRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthCornerPullRight}, 0f },
        { "/mouthStretchLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthStretchLeft }, 0f },
        { "/mouthStretchRight",new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthStretchRight }, 0f },
        { "/mouthUpperUpLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthUpperUpLeft }, 0f },
        { "/mouthUpperUpRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.MouthUpperUpRight }, 0f },
        { "/noseSneerLeft", new HashSet<UnifiedExpressions>() { UnifiedExpressions.NoseSneerLeft }, 0f },
        { "/noseSneerRight", new HashSet<UnifiedExpressions>() { UnifiedExpressions.NoseSneerRight }, 0f },
        { "/tongueOut", new HashSet<UnifiedExpressions>() { UnifiedExpressions.TongueOut }, 0f },
        { "/eyeBlinkLeft", new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeBlinkRight",new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookDownLeft",new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookDownRight",new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookInLeft",new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookInRight",new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookOutLeft", new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookOutRight", new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookUpLeft", new HashSet<UnifiedExpressions>() { }, 0f },
        { "/eyeLookUpRight", new HashSet<UnifiedExpressions>() { }, 0f },
    };
}
