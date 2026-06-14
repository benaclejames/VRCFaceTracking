using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

/// <summary>
/// Opt-in postprocessor that emits calibration-aware face expression helper OSC values.
/// </summary>
public sealed class FaceExpressionOscOutput : TrackingMutation
{
    private bool _isActive;

    public override string Name => "Face Expression OSC Output";

    public override string Description => "Outputs optional calibration-aware face expression helper parameters for VRChat OSC.";

    public override MutationPriority Step => MutationPriority.Postprocessor;

    public override int Order => 1000;

    public override bool IsSaved => true;

    public override bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (!value)
            {
                FaceExpressionOscRuntime.SetDisabled();
            }

            RefreshCalibrationComponents();
        }
    }

    [MutationProperty("Joy Activation Threshold", true, 0f, 1f)]
    public float joyActivationThreshold = 0.5f;

    [MutationProperty("Angry Activation Threshold", true, 0f, 1f)]
    public float angryActivationThreshold = 0.5f;

    [MutationProperty("Sad Activation Threshold", true, 0f, 1f)]
    public float sadActivationThreshold = 0.5f;

    [MutationProperty("Surprise Activation Threshold", true, 0f, 1f)]
    public float surpriseActivationThreshold = 0.5f;

    [MutationProperty("Debug Logging", true)]
    public bool debugLogging = false;

    public FaceExpressionCalibrationData Calibration { get; set; } = new();

    private readonly List<MutationStatus> _calibrationStatusComponents = new();

    private readonly List<MutationStatusAction> _calibrationActionComponents = new();

    public override void Initialize(UnifiedTrackingData data)
    {
        Calibration ??= new FaceExpressionCalibrationData();
        Calibration.EnsureDefaults();
        FaceExpressionOscRuntime.Register(this);
        RefreshCalibrationComponents();

        if (!IsActive)
        {
            FaceExpressionOscRuntime.SetDisabled();
        }
    }

    public override void CreateProperties()
    {
        // Build calibration controls manually, then append reflection-generated settings.
        _calibrationStatusComponents.Clear();
        _calibrationActionComponents.Clear();

        var settingComponents = MutationComponentFactory.CreateComponents(this);
        var components = new ObservableCollection<IMutationComponent>();

        components.Add(new MutationInfo(
            "Make the target expression first, then press its Calibrate button and hold the expression for 1 second. This helper stays disabled until Neutral, Joy, Angry, Sad, and Surprise are all calibrated."));
        AddStatus(components, "Calibration", () => IsCalibrationReady() ? "Ready" : "Required");
        AddStatus(components, "Output", GetOutputStatus);
        AddCalibrationAction(components, "Neutral", FaceExpression.Neutral);
        AddCalibrationAction(components, "Joy", FaceExpression.Joy);
        AddCalibrationAction(components, "Angry", FaceExpression.Angry);
        AddCalibrationAction(components, "Sad", FaceExpression.Sad);
        AddCalibrationAction(components, "Surprise", FaceExpression.Surprise);
        components.Add(new MutationAction("Reset", ResetFaceExpressionCalibration, "Reset", dispatch: GetDispatch()));

        foreach (var component in settingComponents)
        {
            components.Add(component);
        }

        Components = components;
        RefreshCalibrationComponents();
    }

    public override void MutateData(ref UnifiedTrackingData data)
    {
        if (!IsActive)
        {
            FaceExpressionOscRuntime.SetDisabled();
            return;
        }

        FaceExpressionOscRuntime.Update(data, this);
    }

    private void ResetFaceExpressionCalibration()
    {
        FaceExpressionOscRuntime.Register(this);
        FaceExpressionOscRuntime.ResetCalibration();
        _ = SaveSettingsAsync();
    }

    private void BeginCalibration(FaceExpression expression)
    {
        FaceExpressionOscRuntime.Register(this);

        if (!IsActive)
        {
            Logger?.LogWarning("Face expression calibration requested while Face Expression OSC Output is disabled.");
            return;
        }

        FaceExpressionOscRuntime.BeginCalibration(expression);
    }

    public void RefreshCalibrationComponents()
    {
        // These components derive their display state from calibration data and runtime enablement.
        foreach (var status in _calibrationStatusComponents)
        {
            status.Refresh();
        }

        foreach (var action in _calibrationActionComponents)
        {
            action.Refresh();
        }
    }

    private void AddStatus(ObservableCollection<IMutationComponent> components, string name, Func<string> getValue)
    {
        var status = new MutationStatus(name, getValue, GetDispatch());
        _calibrationStatusComponents.Add(status);
        components.Add(status);
    }

    private void AddCalibrationAction(
        ObservableCollection<IMutationComponent> components,
        string name,
        FaceExpression expression)
    {
        var action = new MutationStatusAction(
            name,
            () => IsFaceExpressionCalibrated(expression) ? "Calibrated" : "Not calibrated",
            () => IsFaceExpressionCalibrated(expression) ? "Recalibrate" : "Calibrate",
            () => CanCalibrateFaceExpression(expression),
            () => BeginCalibration(expression),
            GetDispatch());

        _calibrationActionComponents.Add(action);
        components.Add(action);
    }

    private bool IsCalibrationReady()
    {
        return (Calibration ?? new FaceExpressionCalibrationData()).IsReadyForFaceExpressionOsc();
    }

    private string GetOutputStatus()
    {
        if (!IsActive)
        {
            return "Disabled";
        }

        return IsCalibrationReady() ? "Enabled" : "Waiting for calibration";
    }

    private bool IsFaceExpressionCalibrated(FaceExpression expression)
    {
        var calibration = Calibration ?? new FaceExpressionCalibrationData();
        return expression == FaceExpression.Neutral
            ? calibration.HasNeutralBaseline
            : calibration.HasReference(expression);
    }

    private bool CanCalibrateFaceExpression(FaceExpression expression)
    {
        return IsActive;
    }

    public void RequestSave()
    {
        _ = SaveSettingsAsync();
    }

    private async Task SaveSettingsAsync()
    {
        if (LocalSettingsService == null)
        {
            return;
        }

        try
        {
            await LocalSettingsService.SaveSettingAsync(Name, this, true);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to save face expression OSC settings.");
        }
    }

    private Action<Action>? GetDispatch()
    {
        return DispatcherService == null ? null : action => DispatcherService.Run(action);
    }
}
