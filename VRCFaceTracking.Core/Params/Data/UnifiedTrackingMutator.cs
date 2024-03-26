using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking;

/// <summary>
/// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
/// </summary>
public partial class UnifiedTrackingMutator : ObservableObject
{
    private UnifiedTrackingData trackingDataBuffer = new();
    [SavedSetting("Mutations", default, true)]
    public UnifiedMutationConfig mutationData = new();

    [ObservableProperty]
    [property: SavedSetting("CalibrationWeight", 0.2f)]
    private float _calibrationWeight;
        
    [ObservableProperty]
    [property: SavedSetting("ContinuousCalibrationEnabled")]
    private bool _continuousCalibration;

    public bool SmoothingMode = false;

    [ObservableProperty]
    [property: SavedSetting("CalibrationEnabled")]
    private bool _enabled;

    private readonly ILogger<UnifiedTrackingMutator> _logger;
    private readonly ILocalSettingsService _localSettingsService;

    public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, ILocalSettingsService localSettingsService)
    {
        UnifiedTracking.Mutator = this;
        _logger = logger;
        _localSettingsService = localSettingsService;
            
        Enabled = false;
        ContinuousCalibration = true;
        CalibrationWeight = 0.2f;
    }

    static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

    private void Calibrate(ref UnifiedTrackingData inputData, float calibrationWeight)
    {
        if (!Enabled) 
            return;

        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (inputData.Shapes[i].Weight <= 0.0f)
                continue;
            if (calibrationWeight > 0.0f && inputData.Shapes[i].Weight > mutationData.ShapeMutations[i].Ceil) // Calibrator
                mutationData.ShapeMutations[i].Ceil = SimpleLerp(inputData.Shapes[i].Weight, mutationData.ShapeMutations[i].Ceil, calibrationWeight);
            if (calibrationWeight > 0.0f && inputData.Shapes[i].Weight < mutationData.ShapeMutations[i].Floor)
                mutationData.ShapeMutations[i].Floor = SimpleLerp(inputData.Shapes[i].Weight, mutationData.ShapeMutations[i].Floor, calibrationWeight);

            inputData.Shapes[i].Weight = (inputData.Shapes[i].Weight - mutationData.ShapeMutations[i].Floor) / (mutationData.ShapeMutations[i].Ceil - mutationData.ShapeMutations[i].Floor);
        }
    }

    private void ApplyCalibrator(ref UnifiedTrackingData inputData)
        => Calibrate(ref inputData, Enabled ? CalibrationWeight : 0.0f);

    private void ApplySmoothing(ref UnifiedTrackingData input)
    {
        // Example of applying smoothing to the data within the mutator:
        if (!SmoothingMode) return;

        for (int i = 0; i < input.Shapes.Length; i++)
            input.Shapes[i].Weight =
                SimpleLerp(
                    input.Shapes[i].Weight,
                    trackingDataBuffer.Shapes[i].Weight,
                    mutationData.ShapeMutations[i].SmoothnessMult
                );

        input.Eye.Left.Openness = SimpleLerp(input.Eye.Left.Openness, trackingDataBuffer.Eye.Left.Openness, mutationData.OpennessMutationsConfig.SmoothnessMult);
        input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, trackingDataBuffer.Eye.Left.PupilDiameter_MM, mutationData.PupilMutationsConfig.SmoothnessMult);
        input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, trackingDataBuffer.Eye.Left.Gaze, mutationData.GazeMutationsConfig.SmoothnessMult);

        input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, trackingDataBuffer.Eye.Right.Openness, mutationData.OpennessMutationsConfig.SmoothnessMult);
        input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, trackingDataBuffer.Eye.Right.PupilDiameter_MM, mutationData.PupilMutationsConfig.SmoothnessMult);
        input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, trackingDataBuffer.Eye.Right.Gaze, mutationData.GazeMutationsConfig.SmoothnessMult);
    }

    /// <summary>
    /// Takes in the latest base expression Weight data from modules and mutates into the Weight data for output parameters.
    /// </summary>
    /// <returns> Mutated Expression Data. </returns>
    public UnifiedTrackingData MutateData(UnifiedTrackingData input)
    {
        if (!Enabled && SmoothingMode == false)
            return input;

        UnifiedTrackingData inputBuffer = new UnifiedTrackingData();
        inputBuffer.CopyPropertiesOf(input);

        ApplyCalibrator(ref inputBuffer);
        ApplySmoothing(ref inputBuffer);

        trackingDataBuffer.CopyPropertiesOf(inputBuffer);
        return inputBuffer;
    }

    public async Task InitializeCalibration(int durationMs = 30000)
    {
        _logger.LogInformation("Initialized calibration.");

        UnifiedTracking.Mutator.SetCalibration();

        UnifiedTracking.Mutator.CalibrationWeight = 0.75f;
        UnifiedTracking.Mutator.Enabled = true;

        _logger.LogInformation("Calibrating deep normalization for {durationSec}s.", durationMs / 100);
        await Task.Delay(durationMs);

        UnifiedTracking.Mutator.CalibrationWeight = 0.2f;
        _logger.LogInformation("Fine-tuning normalization. Values will be saved on exit.");
    }

    public void SetCalibration(float floor = 999.0f, float ceiling = 0.0f)
    {
        // Currently eye data does not get parsed by calibration.
        mutationData.PupilMutationsConfig.Ceil = ceiling;
        mutationData.GazeMutationsConfig.Ceil = ceiling;
        mutationData.OpennessMutationsConfig.Ceil = ceiling;
            
        mutationData.PupilMutationsConfig.Floor = floor;
        mutationData.GazeMutationsConfig.Floor = floor;
        mutationData.OpennessMutationsConfig.Floor = floor;

        mutationData.PupilMutationsConfig.Name = "Pupil";
        mutationData.GazeMutationsConfig.Name = "Gaze";
        mutationData.OpennessMutationsConfig.Name = "Openness";

        for (int i = 0; i < mutationData.ShapeMutations.Length; i++)
        {
            mutationData.ShapeMutations[i].Name = ((UnifiedExpressions)i).ToString();
            mutationData.ShapeMutations[i].Ceil = ceiling;
            mutationData.ShapeMutations[i].Floor = floor;
        }
    }

    public void SetSmoothness(float setValue = 0.0f)
    {
        // Currently eye data does not get parsed by calibration.
        mutationData.PupilMutationsConfig.SmoothnessMult = setValue;
        mutationData.GazeMutationsConfig.SmoothnessMult = setValue;
        mutationData.OpennessMutationsConfig.SmoothnessMult = setValue;

        for (int i = 0; i < mutationData.ShapeMutations.Length; i++)
            mutationData.ShapeMutations[i].SmoothnessMult = setValue;
    }

    public async Task SaveCalibration()
    {
        _logger.LogDebug("Saving configuration...");
        await _localSettingsService.Save(this);
    }

    public async void LoadCalibration()
    {
        // Try to load config and propogate data into Unified if they exist.
        _logger.LogDebug("Reading configuration...");
        await _localSettingsService.Load(this);
        _logger.LogDebug("Configuration loaded.");
    }
}