using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;
using VRCFaceTracking.ETVR;

namespace VRCFaceTracking.EyeTrackVR;

public class ETVR_VRCFT : ExtTrackingModule
{
    private ETVR_OSC _etvr;
    private const int DEFAULT_PORT = 9999;

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        _etvr = new ETVR_OSC(Logger, DEFAULT_PORT);
        return (true, false);
    }

    public override void Update()
    {
        var fakeWiden = Remap(
            _etvr.EyeDataWithAddress["/avatar/parameters/EyesY"],
            0f, 1f,
            0f, 0.33f);

        UnifiedTracking.Data.Eye.Left.Gaze = new Vector2(
            Math.Clamp(_etvr.EyeDataWithAddress["/avatar/parameters/LeftEyeX"], -1f, 1f),
            Math.Clamp(_etvr.EyeDataWithAddress["/avatar/parameters/EyesY"], -1f, 1f));
        UnifiedTracking.Data.Eye.Left.PupilDiameter_MM = _etvr.EyeDataWithAddress["/avatar/parameters/EyesDilation"];
        UnifiedTracking.Data.Eye.Left.Openness = _etvr.EyeDataWithAddress["/avatar/parameters/LeftEyeLidExpandedSqueeze"];
        UnifiedTracking.Data.Shapes[(int) UnifiedExpressions.EyeWideLeft].Weight = fakeWiden;

        UnifiedTracking.Data.Eye.Left.Gaze = new Vector2(
            Math.Clamp(_etvr.EyeDataWithAddress["/avatar/parameters/RightEyeX"], -1f, 1f),
            Math.Clamp(_etvr.EyeDataWithAddress["/avatar/parameters/EyesY"], -1f, 1f));
        UnifiedTracking.Data.Eye.Left.PupilDiameter_MM = _etvr.EyeDataWithAddress["/avatar/parameters/EyesDilation"];
        UnifiedTracking.Data.Eye.Left.Openness = _etvr.EyeDataWithAddress["/avatar/parameters/RightEyeLidExpandedSqueeze"];
        UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight = fakeWiden;
    }

    public override void Teardown()
    {
        _etvr.Teardown();
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
