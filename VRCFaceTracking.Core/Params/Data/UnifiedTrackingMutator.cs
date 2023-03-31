using System;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
    /// </summary>
    public class UnifiedTrackingMutator
    {
        public struct Mutation
        {
            public string Name;
            public float Ceil; // The maximum that the parameter reaches.
            public float Floor; // the minimum that the parameter reaches.
            //public float SigmoidMult; // How much should this parameter be affected by the sigmoid function. This makes the parameter act more like a toggle.
            //public float LogitMult; // How much should this parameter be affected by the logit (inverse of sigmoid) function. This makes the parameter act more within the normalized range.
            public float SmoothnessMult; // How much should this parameter be affected by the smoothing function.
        }

        public class MutationData
        {
            public Mutation[] ShapeMutations = new Mutation[(int)UnifiedExpressions.Max + 1];
            public Mutation GazeMutations, OpennessMutations, PupilMutations = new Mutation();
        }

        private UnifiedTrackingData trackingDataBuffer = new UnifiedTrackingData();
        public MutationData mutationData = new MutationData();

        public CalibratorState CalibratorMode = CalibratorState.Inactive;
        public float CalibrationWeight;

        public bool SmoothingMode = false;

        /// <summary>
        /// Represents the state of calibration within the mutator.
        /// </summary>
        public enum CalibratorState
        {
            Inactive,
            Calibrating,
            Calibrated
        }

        private static ILogger<UnifiedTrackingMutator> _logger;
        public static void InitializeLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger<UnifiedTrackingMutator>();
        }

        static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

        private void Calibrate(ref UnifiedTrackingData inputData, float calibrationWeight)
        {
            if (CalibratorMode == CalibratorState.Inactive) 
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
            => Calibrate(ref inputData, CalibratorMode == CalibratorState.Calibrating ? CalibrationWeight : 0.0f);

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

            input.Eye.Left.Openness = SimpleLerp(input.Eye.Left.Openness, trackingDataBuffer.Eye.Left.Openness, mutationData.OpennessMutations.SmoothnessMult);
            input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, trackingDataBuffer.Eye.Left.PupilDiameter_MM, mutationData.PupilMutations.SmoothnessMult);
            input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, trackingDataBuffer.Eye.Left.Gaze, mutationData.GazeMutations.SmoothnessMult);

            input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, trackingDataBuffer.Eye.Right.Openness, mutationData.OpennessMutations.SmoothnessMult);
            input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, trackingDataBuffer.Eye.Right.PupilDiameter_MM, mutationData.PupilMutations.SmoothnessMult);
            input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, trackingDataBuffer.Eye.Right.Gaze, mutationData.GazeMutations.SmoothnessMult);
        }

        /// <summary>
        /// Takes in the latest base expression Weight data from modules and mutates into the Weight data for output parameters.
        /// </summary>
        /// <returns> Mutated Expression Data. </returns>
        public UnifiedTrackingData MutateData(UnifiedTrackingData input)
        {
            if (CalibratorMode == CalibratorState.Inactive && SmoothingMode == false)
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
                UnifiedTracking.Mutator.CalibratorMode = CalibratorState.Calibrating;

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
            mutationData.PupilMutations.Ceil = ceiling;
            mutationData.GazeMutations.Ceil = ceiling;
            mutationData.OpennessMutations.Ceil = ceiling;
            
            mutationData.PupilMutations.Floor = floor;
            mutationData.GazeMutations.Floor = floor;
            mutationData.OpennessMutations.Floor = floor;

            mutationData.PupilMutations.Name = "Pupil";
            mutationData.GazeMutations.Name = "Gaze";
            mutationData.OpennessMutations.Name = "Openness";

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
            mutationData.PupilMutations.SmoothnessMult = setValue;
            mutationData.GazeMutations.SmoothnessMult = setValue;
            mutationData.OpennessMutations.SmoothnessMult = setValue;

            for (int i = 0; i < mutationData.ShapeMutations.Length; i++)
                mutationData.ShapeMutations[i].SmoothnessMult = setValue;
        }
    }
}
