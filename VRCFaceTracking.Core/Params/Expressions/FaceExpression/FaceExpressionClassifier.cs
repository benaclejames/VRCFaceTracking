namespace VRCFaceTracking.Core.Params.Expressions;

/// <summary>
/// Final helper OSC classification result.
/// </summary>
public readonly record struct FaceExpressionResult(FaceExpression FaceExpression, int Index, float Power);

/// <summary>
/// Template similarity scores before thresholding and switch smoothing.
/// </summary>
public readonly record struct FaceExpressionRawScores(float Joy, float Angry, float Sad, float Surprise);

/// <summary>
/// Converts calibrated feature templates into expression scores and a selected label.
/// </summary>
public static class FaceExpressionClassifier
{
    private const float ReferenceMinNorm = 0.001f;

    public static FaceExpressionRawScores GetReferenceScores(FaceExpressionFeatureVector rawFeatures, FaceExpressionCalibrationData calibration)
    {
        var features = calibration.HasNeutralBaseline
            ? rawFeatures.SubtractBaseline(calibration.NeutralBaseline)
            : rawFeatures;

        return new FaceExpressionRawScores(
            GetReferenceScore(features, GetReferenceDelta(calibration, FaceExpression.Joy)),
            GetReferenceScore(features, GetReferenceDelta(calibration, FaceExpression.Angry)),
            GetReferenceScore(features, GetReferenceDelta(calibration, FaceExpression.Sad)),
            GetReferenceScore(features, GetReferenceDelta(calibration, FaceExpression.Surprise)));
    }

    public static FaceExpressionResult Classify(
        FaceExpressionRawScores scores,
        float joyActivationThreshold,
        float angryActivationThreshold,
        float sadActivationThreshold,
        float surpriseActivationThreshold)
    {
        var expression = FaceExpression.Joy;
        var maxScore = scores.Joy;
        var activationThreshold = joyActivationThreshold;

        if (scores.Angry > maxScore)
        {
            expression = FaceExpression.Angry;
            maxScore = scores.Angry;
            activationThreshold = angryActivationThreshold;
        }

        if (scores.Sad > maxScore)
        {
            expression = FaceExpression.Sad;
            maxScore = scores.Sad;
            activationThreshold = sadActivationThreshold;
        }

        if (scores.Surprise > maxScore)
        {
            expression = FaceExpression.Surprise;
            maxScore = scores.Surprise;
            activationThreshold = surpriseActivationThreshold;
        }

        if (maxScore < Math.Clamp(activationThreshold, 0f, 1f))
        {
            var neutralScore = Math.Clamp(1f - maxScore, 0f, 1f);
            return new FaceExpressionResult(FaceExpression.Neutral, (int)FaceExpression.Neutral, neutralScore);
        }

        return new FaceExpressionResult(expression, (int)expression, Math.Clamp(maxScore, 0f, 1f));
    }

    public static float GetScoreForFaceExpression(FaceExpressionRawScores scores, FaceExpression expression)
    {
        return expression switch
        {
            FaceExpression.Joy => scores.Joy,
            FaceExpression.Angry => scores.Angry,
            FaceExpression.Sad => scores.Sad,
            FaceExpression.Surprise => scores.Surprise,
            _ => Math.Clamp(1f - Math.Max(Math.Max(scores.Joy, scores.Angry), Math.Max(scores.Sad, scores.Surprise)), 0f, 1f),
        };
    }

    private static float GetReferenceScore(
        FaceExpressionFeatureVector features,
        FaceExpressionFeatureVector reference)
    {
        // Score by direction similarity, then scale down weak expressions that point the same way.
        var currentNorm = GetVectorNorm(features);
        var referenceNorm = GetVectorNorm(reference);
        if (currentNorm < ReferenceMinNorm || referenceNorm < ReferenceMinNorm)
        {
            return 0f;
        }

        var cosine = GetDotProduct(features, reference) / (currentNorm * referenceNorm);
        var strength = Math.Clamp(currentNorm / referenceNorm, 0f, 1f);
        return ClampScore(cosine * strength);
    }

    private static FaceExpressionFeatureVector GetReferenceDelta(FaceExpressionCalibrationData calibration, FaceExpression expression)
    {
        var rawReference = calibration.GetRawReference(expression);
        return calibration.HasNeutralBaseline
            ? rawReference.SubtractBaseline(calibration.NeutralBaseline)
            : rawReference;
    }

    private static float GetVectorNorm(FaceExpressionFeatureVector features)
    {
        return MathF.Sqrt(GetDotProduct(features, features));
    }

    private static float GetDotProduct(
        FaceExpressionFeatureVector left,
        FaceExpressionFeatureVector right)
    {
        return
            Product(left.Smile, right.Smile) +
            Product(left.SadMouth, right.SadMouth) +
            Product(left.BrowDown, right.BrowDown) +
            Product(left.BrowUp, right.BrowUp) +
            Product(left.BrowInnerUp, right.BrowInnerUp) +
            Product(left.EyeWide, right.EyeWide) +
            Product(left.EyeSquint, right.EyeSquint) +
            Product(left.CheekSquint, right.CheekSquint) +
            Product(left.MouthOpen, right.MouthOpen) +
            Product(left.JawOpen, right.JawOpen) +
            Product(left.MouthPress, right.MouthPress) +
            Product(left.MouthTightener, right.MouthTightener) +
            Product(left.NoseSneer, right.NoseSneer);
    }

    private static float Product(float left, float right)
    {
        return Math.Clamp(left, 0f, 1f) * Math.Clamp(right, 0f, 1f);
    }

    private static float ClampScore(float score) => Math.Clamp(score, 0f, 1f);
}
