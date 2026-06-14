using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Data.Mutation;

namespace VRCFaceTracking.Core.Params.Expressions;

/// <summary>
/// Thread-safe read model used by OSC parameter getters.
/// </summary>
public readonly record struct FaceExpressionRuntimeSnapshot(bool Enabled, FaceExpressionResult CurrentResult);

/// <summary>
/// Owns calibration capture, classification, and runtime state for the helper OSC output.
/// </summary>
public static class FaceExpressionOscRuntime
{
    private const float CalibrationDurationSeconds = 1f;
    private const float HoldTimeSeconds = 0.2f;
    private const float SwitchMargin = 0.08f;

    private static readonly object Lock = new();

    private static FaceExpressionOscOutput? _owner;
    private static bool _enabled;
    private static bool _saveRequested;
    private static FaceExpressionResult _currentResult = new(FaceExpression.Neutral, 0, 0f);
    private static FaceExpression _candidateFaceExpression = FaceExpression.Neutral;
    private static long _candidateSinceTimestamp;
    private static CalibrationSession? _calibrationSession;

    public static void Register(FaceExpressionOscOutput owner)
    {
        lock (Lock)
        {
            _owner = owner;
        }
    }

    public static void SetDisabled()
    {
        lock (Lock)
        {
            SetDisabledUnderLock();
            _calibrationSession = null;
        }
    }

    public static FaceExpressionRuntimeSnapshot GetSnapshot()
    {
        lock (Lock)
        {
            var ownerEnabled = _owner?.IsActive == true;
            var mutatorEnabled = UnifiedTracking.Mutator?.Enabled == true;
            if (!_enabled || !ownerEnabled || !mutatorEnabled)
            {
                return new FaceExpressionRuntimeSnapshot(false, new FaceExpressionResult(FaceExpression.Neutral, 0, 0f));
            }

            return new FaceExpressionRuntimeSnapshot(_enabled, _currentResult);
        }
    }

    public static void Update(UnifiedTrackingData data, FaceExpressionOscOutput owner)
    {
        FaceExpressionOscOutput? ownerToSave = null;

        lock (Lock)
        {
            _owner = owner;
            if (!owner.IsActive)
            {
                SetDisabledUnderLock();
                _calibrationSession = null;
                if (ConsumeSaveRequestedUnderLock())
                {
                    ownerToSave = owner;
                }
            }
            else
            {
                var calibration = owner.Calibration;
                var rawFeatures = FaceExpressionFeatureExtractor.Extract(data);

                // Calibration and classification share the same extracted feature vector for this frame.
                ProcessCalibrationUnderLock(rawFeatures);

                if (!calibration.IsReadyForFaceExpressionOsc())
                {
                    SetDisabledUnderLock();
                    if (ConsumeSaveRequestedUnderLock())
                    {
                        ownerToSave = owner;
                    }
                }
                else
                {
                    _enabled = true;
                    var scores = FaceExpressionClassifier.GetReferenceScores(rawFeatures, calibration);

                    var candidate = FaceExpressionClassifier.Classify(
                        scores,
                        owner.joyActivationThreshold,
                        owner.angryActivationThreshold,
                        owner.sadActivationThreshold,
                        owner.surpriseActivationThreshold);
                    _currentResult = ApplySwitchRulesUnderLock(candidate, scores);

                    if (ConsumeSaveRequestedUnderLock())
                    {
                        ownerToSave = owner;
                    }
                }
            }
        }

        ownerToSave?.RequestSave();
    }

    public static void BeginCalibration(FaceExpression expression)
    {
        lock (Lock)
        {
            if (_owner == null)
            {
                return;
            }

            _owner.Calibration.EnsureDefaults();
            _calibrationSession = new CalibrationSession(expression, CalibrationDurationSeconds);
            _owner.Logger?.LogInformation("Starting face expression calibration: {expression}", expression);
        }
    }

    public static void ResetCalibration()
    {
        lock (Lock)
        {
            _owner?.Calibration.Reset();
            _calibrationSession = null;
            SetDisabledUnderLock();
            _owner?.RefreshCalibrationComponents();
            _owner?.Logger?.LogInformation("Reset face expression calibration");
        }
    }

    private static void SetDisabledUnderLock()
    {
        _enabled = false;
        _candidateFaceExpression = FaceExpression.Neutral;
        _candidateSinceTimestamp = 0;
        _currentResult = new FaceExpressionResult(FaceExpression.Neutral, 0, 0f);
    }

    private static FaceExpressionResult ApplySwitchRulesUnderLock(FaceExpressionResult candidate, FaceExpressionRawScores scores)
    {
        // Hold and margin prevent rapid label flicker when multiple templates score similarly.
        if (_owner == null)
        {
            return new FaceExpressionResult(FaceExpression.Neutral, 0, 0f);
        }

        if (candidate.FaceExpression == _currentResult.FaceExpression)
        {
            _candidateFaceExpression = candidate.FaceExpression;
            _candidateSinceTimestamp = Stopwatch.GetTimestamp();
            return candidate;
        }

        if (!CanSwitchUnderLock(candidate.FaceExpression, scores))
        {
            return BuildResultForFaceExpression(_currentResult.FaceExpression, scores);
        }

        var now = Stopwatch.GetTimestamp();
        if (_candidateFaceExpression != candidate.FaceExpression)
        {
            _candidateFaceExpression = candidate.FaceExpression;
            _candidateSinceTimestamp = now;
            return BuildResultForFaceExpression(_currentResult.FaceExpression, scores);
        }

        var elapsedSeconds = (now - _candidateSinceTimestamp) / (double)Stopwatch.Frequency;
        if (elapsedSeconds < HoldTimeSeconds)
        {
            return BuildResultForFaceExpression(_currentResult.FaceExpression, scores);
        }

        if (_owner.debugLogging)
        {
            _owner.Logger?.LogDebug("Face expression classified {expression} with power {power}", candidate.FaceExpression, candidate.Power);
        }

        return candidate;
    }

    private static bool CanSwitchUnderLock(FaceExpression candidateFaceExpression, FaceExpressionRawScores scores)
    {
        if (_owner == null || candidateFaceExpression == FaceExpression.Neutral || _currentResult.FaceExpression == FaceExpression.Neutral)
        {
            return true;
        }

        var candidateScore = FaceExpressionClassifier.GetScoreForFaceExpression(scores, candidateFaceExpression);
        var currentScore = FaceExpressionClassifier.GetScoreForFaceExpression(scores, _currentResult.FaceExpression);

        return candidateScore >= currentScore + SwitchMargin;
    }

    private static FaceExpressionResult BuildResultForFaceExpression(FaceExpression expression, FaceExpressionRawScores scores)
    {
        var score = FaceExpressionClassifier.GetScoreForFaceExpression(scores, expression);
        return new FaceExpressionResult(expression, (int)expression, Math.Clamp(score, 0f, 1f));
    }

    private static void ProcessCalibrationUnderLock(FaceExpressionFeatureVector rawFeatures)
    {
        // Accumulate a short sample window so templates are less sensitive to single-frame noise.
        if (_owner == null || _calibrationSession == null)
        {
            return;
        }

        _calibrationSession.FeatureSum.Add(rawFeatures);
        _calibrationSession.SampleCount++;

        var elapsedSeconds = (Stopwatch.GetTimestamp() - _calibrationSession.StartTimestamp) / (double)Stopwatch.Frequency;
        if (elapsedSeconds < _calibrationSession.DurationSeconds)
        {
            return;
        }

        CompleteCalibrationUnderLock(_calibrationSession);
        _calibrationSession = null;
        _saveRequested = true;
    }

    private static void CompleteCalibrationUnderLock(CalibrationSession session)
    {
        if (_owner == null)
        {
            return;
        }

        if (session.SampleCount <= 0)
        {
            _owner.Logger?.LogWarning("Face expression calibration completed without samples: {expression}", session.FaceExpression);
            return;
        }

        if (session.FaceExpression == FaceExpression.Neutral)
        {
            _owner.Calibration.NeutralBaseline = session.FeatureSum.Divide(session.SampleCount);
            _owner.Calibration.HasNeutralBaseline = true;
            _owner.RefreshCalibrationComponents();
            _owner.Logger?.LogInformation("Completed face expression calibration: Neutral");
            return;
        }

        var reference = session.FeatureSum.Divide(session.SampleCount);
        _owner.Calibration.SetRawReference(session.FaceExpression, reference, session.SampleCount);
        _owner.RefreshCalibrationComponents();
        _owner.Logger?.LogInformation(
            "Completed face expression calibration: {expression}, reference samples = {samples}",
            session.FaceExpression,
            session.SampleCount);
    }

    private static bool ConsumeSaveRequestedUnderLock()
    {
        if (!_saveRequested)
        {
            return false;
        }

        _saveRequested = false;
        return true;
    }

    private sealed class CalibrationSession
    {
        public CalibrationSession(FaceExpression expression, float durationSeconds)
        {
            FaceExpression = expression;
            DurationSeconds = durationSeconds;
            StartTimestamp = Stopwatch.GetTimestamp();
        }

        public FaceExpression FaceExpression { get; }

        public float DurationSeconds { get; }

        public long StartTimestamp { get; }

        public int SampleCount { get; set; }

        public FaceExpressionFeatureVector FeatureSum;
    }
}
