using System.Reflection;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Babble;
public class BabbleVRC : ExtTrackingModule
{
    private BabbleOSC babbleOSC;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (false, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        babbleOSC = new BabbleOSC(Logger);

        List<Stream> streams = new List<Stream>();
        Assembly a = Assembly.GetExecutingAssembly();
        var hmdStream = a.GetManifestResourceStream
            ("VRCFaceTracking.Babble.BabbleLogo.png");
        streams.Add(hmdStream);
        ModuleInformation = new ModuleMetadata()
        {
            Name = "Project Babble Face Tracking\nInference Model v2.0.0",
            StaticImages = streams
        };

        return (false, true);
    }

    public override void Teardown() => babbleOSC.Teardown();
    public override void Update()
    {
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.TongueOut].Weight = babbleOSC.BabbleExpressionMap["/tongueOut"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen].Weight = babbleOSC.BabbleExpressionMap["/jawOpen"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawLeft].Weight = -babbleOSC.BabbleExpressionMap["/jawLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawRight].Weight = -babbleOSC.BabbleExpressionMap["/jawRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawForward].Weight = babbleOSC.BabbleExpressionMap["/jawForward"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight = -babbleOSC.BabbleExpressionMap["/noseSneerLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight = -babbleOSC.BabbleExpressionMap["/noseSneerRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerLeft].Weight = babbleOSC.BabbleExpressionMap["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerLowerRight].Weight = babbleOSC.BabbleExpressionMap["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperLeft].Weight = babbleOSC.BabbleExpressionMap["/mouthPucker"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipPuckerUpperRight].Weight = babbleOSC.BabbleExpressionMap["/mouthPucker"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerLeft].Weight = babbleOSC.BabbleExpressionMap["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelLowerRight].Weight = babbleOSC.BabbleExpressionMap["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperLeft].Weight = babbleOSC.BabbleExpressionMap["/mouthFunnel"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.LipFunnelUpperRight].Weight = babbleOSC.BabbleExpressionMap["/mouthFunnel"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthUpperUpLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthUpperUpRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthLowerDownLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthLowerDownRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthPressLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthPressRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthStretchLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthStretchRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthDimpleLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthDimpleRight"];

        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthSmileLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthCornerPullRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthSmileRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].Weight = -babbleOSC.BabbleExpressionMap["/mouthFrownLeft"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.MouthFrownRight].Weight = -babbleOSC.BabbleExpressionMap["/mouthFrownRight"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].Weight = babbleOSC.BabbleExpressionMap["/cheekPuff"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.CheekPuffRight].Weight = babbleOSC.BabbleExpressionMap["/cheekPuff"];
    }
}