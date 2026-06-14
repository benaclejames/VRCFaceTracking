using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Params.Expressions;

/// <summary>
/// Reduced set of expression features used for calibration and template matching.
/// </summary>
public struct FaceExpressionFeatureVector
{
    public float Smile { get; set; }
    public float SadMouth { get; set; }
    public float BrowDown { get; set; }
    public float BrowUp { get; set; }
    public float BrowInnerUp { get; set; }
    public float EyeWide { get; set; }
    public float EyeSquint { get; set; }
    public float CheekSquint { get; set; }
    public float MouthOpen { get; set; }
    public float JawOpen { get; set; }
    public float MouthPress { get; set; }
    public float MouthTightener { get; set; }
    public float NoseSneer { get; set; }

    public void Add(FaceExpressionFeatureVector other)
    {
        Smile += other.Smile;
        SadMouth += other.SadMouth;
        BrowDown += other.BrowDown;
        BrowUp += other.BrowUp;
        BrowInnerUp += other.BrowInnerUp;
        EyeWide += other.EyeWide;
        EyeSquint += other.EyeSquint;
        CheekSquint += other.CheekSquint;
        MouthOpen += other.MouthOpen;
        JawOpen += other.JawOpen;
        MouthPress += other.MouthPress;
        MouthTightener += other.MouthTightener;
        NoseSneer += other.NoseSneer;
    }

    public FaceExpressionFeatureVector Divide(float divisor)
    {
        if (divisor <= 0f)
        {
            return this;
        }

        return new FaceExpressionFeatureVector
        {
            Smile = Smile / divisor,
            SadMouth = SadMouth / divisor,
            BrowDown = BrowDown / divisor,
            BrowUp = BrowUp / divisor,
            BrowInnerUp = BrowInnerUp / divisor,
            EyeWide = EyeWide / divisor,
            EyeSquint = EyeSquint / divisor,
            CheekSquint = CheekSquint / divisor,
            MouthOpen = MouthOpen / divisor,
            JawOpen = JawOpen / divisor,
            MouthPress = MouthPress / divisor,
            MouthTightener = MouthTightener / divisor,
            NoseSneer = NoseSneer / divisor,
        };
    }

    public FaceExpressionFeatureVector SubtractBaseline(FaceExpressionFeatureVector baseline)
    {
        // Baseline subtraction keeps only expression movement above the user's neutral face.
        return new FaceExpressionFeatureVector
        {
            Smile = ClampPositive(Smile - baseline.Smile),
            SadMouth = ClampPositive(SadMouth - baseline.SadMouth),
            BrowDown = ClampPositive(BrowDown - baseline.BrowDown),
            BrowUp = ClampPositive(BrowUp - baseline.BrowUp),
            BrowInnerUp = ClampPositive(BrowInnerUp - baseline.BrowInnerUp),
            EyeWide = ClampPositive(EyeWide - baseline.EyeWide),
            EyeSquint = ClampPositive(EyeSquint - baseline.EyeSquint),
            CheekSquint = ClampPositive(CheekSquint - baseline.CheekSquint),
            MouthOpen = ClampPositive(MouthOpen - baseline.MouthOpen),
            JawOpen = ClampPositive(JawOpen - baseline.JawOpen),
            MouthPress = ClampPositive(MouthPress - baseline.MouthPress),
            MouthTightener = ClampPositive(MouthTightener - baseline.MouthTightener),
            NoseSneer = ClampPositive(NoseSneer - baseline.NoseSneer),
        };
    }

    private static float ClampPositive(float value) => Math.Clamp(value, 0f, 1f);
}

/// <summary>
/// Extracts stable helper features from the full UnifiedTracking shape set.
/// </summary>
public static class FaceExpressionFeatureExtractor
{
    private const float SmileCornerPullWeight = 0.8f;
    private const float SmileCornerSlantWeight = 0.2f;
    private const float SadMouthStretchFallbackWeight = 0.35f;
    private const float BrowLowererWeight = 0.75f;
    private const float BrowPinchWeight = 0.25f;
    private const float BrowOuterUpWeight = 0.6f;
    private const float BrowInnerUpWeight = 0.4f;
    private const float MouthOpenJawWeight = 0.5f;
    private const float MouthOpenLipWeight = 0.25f;

    public static FaceExpressionFeatureVector Extract(UnifiedTrackingData data)
    {
        var jawOpen = W(data, UnifiedExpressions.JawOpen);

        return new FaceExpressionFeatureVector
        {
            Smile = Avg(
                W(data, UnifiedExpressions.MouthCornerPullLeft) * SmileCornerPullWeight +
                W(data, UnifiedExpressions.MouthCornerSlantLeft) * SmileCornerSlantWeight,
                W(data, UnifiedExpressions.MouthCornerPullRight) * SmileCornerPullWeight +
                W(data, UnifiedExpressions.MouthCornerSlantRight) * SmileCornerSlantWeight),

            SadMouth = Avg(
                Max(W(data, UnifiedExpressions.MouthFrownLeft), W(data, UnifiedExpressions.MouthStretchLeft) * SadMouthStretchFallbackWeight),
                Max(W(data, UnifiedExpressions.MouthFrownRight), W(data, UnifiedExpressions.MouthStretchRight) * SadMouthStretchFallbackWeight)),

            BrowDown = Avg(
                W(data, UnifiedExpressions.BrowLowererLeft) * BrowLowererWeight +
                W(data, UnifiedExpressions.BrowPinchLeft) * BrowPinchWeight,
                W(data, UnifiedExpressions.BrowLowererRight) * BrowLowererWeight +
                W(data, UnifiedExpressions.BrowPinchRight) * BrowPinchWeight),

            BrowUp = Avg(
                W(data, UnifiedExpressions.BrowOuterUpLeft) * BrowOuterUpWeight +
                W(data, UnifiedExpressions.BrowInnerUpLeft) * BrowInnerUpWeight,
                W(data, UnifiedExpressions.BrowOuterUpRight) * BrowOuterUpWeight +
                W(data, UnifiedExpressions.BrowInnerUpRight) * BrowInnerUpWeight),

            BrowInnerUp = Avg(
                W(data, UnifiedExpressions.BrowInnerUpLeft),
                W(data, UnifiedExpressions.BrowInnerUpRight)),

            EyeWide = Avg(
                W(data, UnifiedExpressions.EyeWideLeft),
                W(data, UnifiedExpressions.EyeWideRight)),

            EyeSquint = Avg(
                W(data, UnifiedExpressions.EyeSquintLeft),
                W(data, UnifiedExpressions.EyeSquintRight)),

            CheekSquint = Avg(
                W(data, UnifiedExpressions.CheekSquintLeft),
                W(data, UnifiedExpressions.CheekSquintRight)),

            JawOpen = jawOpen,

            MouthOpen = Math.Clamp(
                jawOpen * MouthOpenJawWeight +
                Avg(
                    W(data, UnifiedExpressions.MouthUpperUpLeft) + W(data, UnifiedExpressions.MouthLowerDownLeft),
                    W(data, UnifiedExpressions.MouthUpperUpRight) + W(data, UnifiedExpressions.MouthLowerDownRight)) * MouthOpenLipWeight,
                0f,
                1f),

            MouthPress = Avg(
                W(data, UnifiedExpressions.MouthPressLeft),
                W(data, UnifiedExpressions.MouthPressRight)),

            MouthTightener = Avg(
                W(data, UnifiedExpressions.MouthTightenerLeft),
                W(data, UnifiedExpressions.MouthTightenerRight)),

            NoseSneer = Avg(
                W(data, UnifiedExpressions.NoseSneerLeft),
                W(data, UnifiedExpressions.NoseSneerRight)),
        };
    }

    private static float W(UnifiedTrackingData data, UnifiedExpressions shape)
    {
        // Modules may produce invalid floats; keep the helper output bounded and deterministic.
        var value = data.Shapes[(int)shape].Weight;
        return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Math.Clamp(value, 0f, 1f);
    }

    private static float Avg(float a, float b) => (a + b) * 0.5f;

    private static float Max(float a, float b) => Math.Max(a, b);
}
