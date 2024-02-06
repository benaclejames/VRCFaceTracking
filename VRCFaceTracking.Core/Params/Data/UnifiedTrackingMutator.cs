using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
    /// </summary>
    public class UnifiedTrackingMutator : INotifyPropertyChanged
    {
        private UnifiedTrackingData trackingDataBuffer = new();
        public UnifiedMutationConfig mutationData = new();

        private float _calibrationWeight;
        public float CalibrationWeight
        {
            get => _calibrationWeight;
            set => SetField(ref _calibrationWeight, value);
        }
        
        private bool _continuousCalibration;
        public bool ContinuousCalibration
        {
            get => _continuousCalibration;
            set => SetField(ref _continuousCalibration, value);
        }

        public bool SmoothingMode = false;

        private bool _enabled;
        public bool Enabled 
        {
            get => _enabled;
            set => SetField(ref _enabled, value);
        }

        private readonly ILogger<UnifiedTrackingMutator> _logger;
        private readonly IDispatcherService _dispatcherService;
        private readonly ILocalSettingsService _localSettingsService;

        public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, IDispatcherService dispatcherService, ILocalSettingsService localSettingsService)
        {
            UnifiedTracking.Mutator = this;
            _logger = logger;
            _dispatcherService = dispatcherService;
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

        public void InitializeCalibration()
        {
            Thread _thread = new Thread(() =>
            {
                _logger.LogInformation("Initialized calibration.");

                UnifiedTracking.Mutator.SetCalibration();

                UnifiedTracking.Mutator.CalibrationWeight = 0.75f;
                UnifiedTracking.Mutator.Enabled = true;

                _logger.LogInformation("Calibrating deep normalization for 30s.");
                Thread.Sleep(30000);

                UnifiedTracking.Mutator.CalibrationWeight = 0.2f;
                _logger.LogInformation("Fine-tuning normalization. Values will be saved on exit.");

            });
            _thread.Start();
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
            await _localSettingsService.SaveSettingAsync("CalibrationEnabled", Enabled);
            await _localSettingsService.SaveSettingAsync("CalibrationWeight", CalibrationWeight);
            await _localSettingsService.SaveSettingAsync("ContinuousCalibrationEnabled", ContinuousCalibration);
            await _localSettingsService.SaveSettingAsync("Mutations", mutationData, true);
        }

        public async void LoadCalibration()
        {
            // Try to load config and propogate data into Unified if they exist.
            _logger.LogDebug("Reading configuration...");
            Enabled = await _localSettingsService.ReadSettingAsync<bool>("CalibrationEnabled");
            CalibrationWeight = await _localSettingsService.ReadSettingAsync<float>("CalibrationWeight", 0.2f);
            ContinuousCalibration = await _localSettingsService.ReadSettingAsync<bool>("ContinuousCalibrationEnabled", true);
            mutationData = await _localSettingsService.ReadSettingAsync<UnifiedMutationConfig>("Mutation", new());
            _logger.LogDebug("Configuration loaded.");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            _dispatcherService.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
