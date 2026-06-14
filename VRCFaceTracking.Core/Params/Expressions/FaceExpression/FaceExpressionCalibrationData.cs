using System.Runtime.Serialization;

namespace VRCFaceTracking.Core.Params.Expressions;

/// <summary>
/// Stores the user's neutral baseline and per-expression reference templates.
/// </summary>
public sealed class FaceExpressionCalibrationData
{
    public FaceExpressionFeatureVector NeutralBaseline { get; set; }

    public bool HasNeutralBaseline { get; set; }

    public FaceExpressionReferenceCalibrationData JoyReference { get; set; } = new();
    public FaceExpressionReferenceCalibrationData AngryReference { get; set; } = new();
    public FaceExpressionReferenceCalibrationData SadReference { get; set; } = new();
    public FaceExpressionReferenceCalibrationData SurpriseReference { get; set; } = new();

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        EnsureDefaults();
    }

    public bool IsReadyForFaceExpressionOsc()
    {
        return HasNeutralBaseline && HasCompleteFaceExpressionReferences();
    }

    /// <summary>
    /// Checks every non-neutral template; the neutral baseline is checked by IsReadyForFaceExpressionOsc.
    /// </summary>
    public bool HasCompleteFaceExpressionReferences()
    {
        return JoyReference.HasReference &&
            AngryReference.HasReference &&
            SadReference.HasReference &&
            SurpriseReference.HasReference;
    }

    public bool HasReference(FaceExpression expression)
    {
        return TryGetReference(expression)?.HasReference == true;
    }

    public FaceExpressionFeatureVector GetRawReference(FaceExpression expression)
    {
        return TryGetReference(expression)?.RawReference ?? default;
    }

    /// <summary>
    /// Stores the caller-provided raw reference vector and its calibration sample count.
    /// </summary>
    public void SetRawReference(FaceExpression expression, FaceExpressionFeatureVector reference, int sampleCount)
    {
        EnsureDefaults();
        var referenceData = TryGetReference(expression);
        if (referenceData == null)
        {
            return;
        }

        referenceData.HasReference = true;
        referenceData.SampleCount = Math.Max(sampleCount, 0);
        referenceData.RawReference = reference;
    }

    public void Reset()
    {
        NeutralBaseline = default;
        HasNeutralBaseline = false;
        ClearFaceExpressionReferences();
    }

    public void ClearFaceExpressionReferences()
    {
        JoyReference = new FaceExpressionReferenceCalibrationData();
        AngryReference = new FaceExpressionReferenceCalibrationData();
        SadReference = new FaceExpressionReferenceCalibrationData();
        SurpriseReference = new FaceExpressionReferenceCalibrationData();
    }

    public void EnsureDefaults()
    {
        JoyReference ??= new FaceExpressionReferenceCalibrationData();
        AngryReference ??= new FaceExpressionReferenceCalibrationData();
        SadReference ??= new FaceExpressionReferenceCalibrationData();
        SurpriseReference ??= new FaceExpressionReferenceCalibrationData();
    }

    private FaceExpressionReferenceCalibrationData? TryGetReference(FaceExpression expression)
    {
        return expression switch
        {
            FaceExpression.Joy => JoyReference,
            FaceExpression.Angry => AngryReference,
            FaceExpression.Sad => SadReference,
            FaceExpression.Surprise => SurpriseReference,
            _ => null,
        };
    }
}

/// <summary>
/// Calibration template and metadata for a single non-neutral expression.
/// </summary>
public sealed class FaceExpressionReferenceCalibrationData
{
    public bool HasReference { get; set; }

    public int SampleCount { get; set; }

    public FaceExpressionFeatureVector RawReference { get; set; }

}
