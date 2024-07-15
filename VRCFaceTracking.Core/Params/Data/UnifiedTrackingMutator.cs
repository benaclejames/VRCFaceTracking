using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Params.Data;

/// <summary>
/// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
/// </summary>
public partial class UnifiedTrackingMutator : ObservableObject
{
    private readonly UnifiedTrackingData _trackingDataBuffer = new();

    private UnifiedMutationConfig _mutationData = new();

    [ObservableProperty]
    [property: SavedSetting("CalibrationWeight", 0.2f)]
    private float _calibrationWeight;
        
    [ObservableProperty]
    [property: SavedSetting("ContinuousCalibrationEnabled", true)]
    private bool _continuousCalibration;

    public bool SmoothingMode = false;

    [ObservableProperty]
    [property: SavedSetting("CalibrationEnabled", true)]
    private bool _enabled;

    private readonly ILogger<UnifiedTrackingMutator> _logger;
    private readonly ILocalSettingsService _localSettingsService;
    private UnifiedTrackingData _inputBuffer;

    public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, ILocalSettingsService localSettingsService)
    {
        UnifiedTracking.Mutator = this;
        _logger = logger;
        _localSettingsService = localSettingsService;
            
        Enabled = false;
        ContinuousCalibration = true;
        CalibrationWeight = 0.2f;
        _inputBuffer = new UnifiedTrackingData();
    }

    static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

    private void Calibrate(ref UnifiedTrackingData inputData, float calibrationWeight)
    {
        if (!Enabled)
        {
            return;
        }

        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (inputData.Shapes[i].Weight <= 0.0f)
            {
                continue;
            }

            if (calibrationWeight > 0.0f && inputData.Shapes[i].Weight > _mutationData.ShapeMutations[i].Ceil) // Calibrator
            {
                _mutationData.ShapeMutations[i].Ceil = SimpleLerp(inputData.Shapes[i].Weight, _mutationData.ShapeMutations[i].Ceil, calibrationWeight);
            }

            if (calibrationWeight > 0.0f && inputData.Shapes[i].Weight < _mutationData.ShapeMutations[i].Floor)
            {
                _mutationData.ShapeMutations[i].Floor = SimpleLerp(inputData.Shapes[i].Weight, _mutationData.ShapeMutations[i].Floor, calibrationWeight);
            }

            inputData.Shapes[i].Weight = (inputData.Shapes[i].Weight - _mutationData.ShapeMutations[i].Floor) / (_mutationData.ShapeMutations[i].Ceil - _mutationData.ShapeMutations[i].Floor);
        }
    }

    private void ApplyCalibrator(ref UnifiedTrackingData inputData)
        => Calibrate(ref inputData, Enabled ? CalibrationWeight : 0.0f);

    private void ApplySmoothing(ref UnifiedTrackingData input)
    {
        // Example of applying smoothing to the data within the mutator:
        if (!SmoothingMode)
        {
            return;
        }

        for (var i = 0; i < input.Shapes.Length; i++)
        {
            input.Shapes[i].Weight =
                SimpleLerp(
                    input.Shapes[i].Weight,
                    _trackingDataBuffer.Shapes[i].Weight,
                    _mutationData.ShapeMutations[i].SmoothnessMult
                );
        }

        input.Eye.Left.Openness = SimpleLerp(input.Eye.Left.Openness, _trackingDataBuffer.Eye.Left.Openness, _mutationData.OpennessMutationsConfig.SmoothnessMult);
        input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, _trackingDataBuffer.Eye.Left.PupilDiameter_MM, _mutationData.PupilMutationsConfig.SmoothnessMult);
        input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, _trackingDataBuffer.Eye.Left.Gaze, _mutationData.GazeMutationsConfig.SmoothnessMult);

        input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, _trackingDataBuffer.Eye.Right.Openness, _mutationData.OpennessMutationsConfig.SmoothnessMult);
        input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, _trackingDataBuffer.Eye.Right.PupilDiameter_MM, _mutationData.PupilMutationsConfig.SmoothnessMult);
        input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, _trackingDataBuffer.Eye.Right.Gaze, _mutationData.GazeMutationsConfig.SmoothnessMult);
    }

    private void ApplyCorrections(ref UnifiedTrackingData input)
    {
        input.Shapes[(int)UnifiedExpressions.MouthClosed].Weight = Math.Min(
            input.Shapes[(int)UnifiedExpressions.MouthClosed].Weight,
            input.Shapes[(int)UnifiedExpressions.JawOpen].Weight);
    }
    
    /// <summary>
    /// Takes in the latest base expression Weight data from modules and mutates into the Weight data for output parameters.
    /// </summary>
    /// <returns> Mutated Expression Data. </returns>
    public UnifiedTrackingData MutateData(UnifiedTrackingData input)
    {
        if (!Enabled && SmoothingMode == false)
        {
            return input;
        }

        _inputBuffer.CopyPropertiesOf(input);

        ApplyCalibrator(ref _inputBuffer);
        ApplySmoothing(ref _inputBuffer);
        ApplyCorrections(ref _inputBuffer);

        _trackingDataBuffer.CopyPropertiesOf(_inputBuffer);
        return _inputBuffer;
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
        _mutationData.PupilMutationsConfig.Ceil = ceiling;
        _mutationData.GazeMutationsConfig.Ceil = ceiling;
        _mutationData.OpennessMutationsConfig.Ceil = ceiling;
            
        _mutationData.PupilMutationsConfig.Floor = floor;
        _mutationData.GazeMutationsConfig.Floor = floor;
        _mutationData.OpennessMutationsConfig.Floor = floor;

        _mutationData.PupilMutationsConfig.Name = "Pupil";
        _mutationData.GazeMutationsConfig.Name = "Gaze";
        _mutationData.OpennessMutationsConfig.Name = "Openness";

        for (var i = 0; i < _mutationData.ShapeMutations.Length; i++)
        {
            _mutationData.ShapeMutations[i].Name = ((UnifiedExpressions)i).ToString();
            _mutationData.ShapeMutations[i].Ceil = ceiling;
            _mutationData.ShapeMutations[i].Floor = floor;
        }
    }

    public void SetSmoothness(float setValue = 0.0f)
    {
        // Currently eye data does not get parsed by calibration.
        _mutationData.PupilMutationsConfig.SmoothnessMult = setValue;
        _mutationData.GazeMutationsConfig.SmoothnessMult = setValue;
        _mutationData.OpennessMutationsConfig.SmoothnessMult = setValue;

        for (var i = 0; i < _mutationData.ShapeMutations.Length; i++)
        {
            _mutationData.ShapeMutations[i].SmoothnessMult = setValue;
        }
    }

    public async Task SaveCalibration()
    {
        _logger.LogDebug("Saving configuration...");
        await _localSettingsService.Save(this);
        await _localSettingsService.SaveSettingAsync("Mutations", _mutationData, true);
        _logger.LogDebug("Configuration saved.");
    }

    public async void LoadCalibration()
    {
        // Try to load config and propogate data into Unified if they exist.
        _logger.LogDebug("Reading configuration...");
        await _localSettingsService.Load(this);
        _mutationData = await _localSettingsService.ReadSettingAsync("Mutations", new UnifiedMutationConfig(), true);
        _logger.LogDebug("Configuration loaded.");
    }
}