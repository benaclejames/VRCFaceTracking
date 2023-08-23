using System.Reflection;

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
        foreach (var exp in babbleOSC.BabbleExpressionMap)
        {
            UnifiedTracking.Data.Shapes[(int)exp.Key].Weight = babbleOSC.BabbleExpressionMap.GetByKey1(exp.Key);
        }
    }
}