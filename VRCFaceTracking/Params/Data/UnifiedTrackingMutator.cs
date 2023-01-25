using System;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Markup;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    public struct Mutation
    {
        public float CalibrationMult; // The amount to multiply this parameter to normalize between 0-1.
        //public float SigmoidMult; // How much should this parameter be affected by the sigmoid function. This makes the parameter act more like a toggle.
        //public float LogitMult; // How much should this parameter be affected by the logit (inverse of sigmoid) function. This makes the parameter act more within the normalized range.
        public float SmoothnessMult; // How much should this parameter be affected by the smoothing function.
    }

    public class MutationData
    {
        public Mutation[] ShapeMutations = new Mutation[(int)UnifiedExpressions.Max + 1];
        public Mutation GazeMutations, OpennessMutations, PupilMutations = new Mutation();
    }

    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
    /// </summary>
    public class UnifiedTrackingMutator
    {
        private static UnifiedTrackingData TrackingDataBuffer = new UnifiedTrackingData();
        public MutationData MutationData = new MutationData();

        public CalibratorState CalibratorMode = CalibratorState.Inactive;
        public float CalibrationWeight;

        public bool SmoothingMode = false;

        static float SimpleLerp(float input, float previousInput, float value)
        {
            return input * (1.0f - value) + previousInput * value;
        }

        static Vector2 SimpleLerp(Vector2 input, Vector2 previousInput, float value)
        {
            return input * (1.0f - value) + previousInput * value;
        }

        /// <summary>
        /// Represents the state of calibration within the mutator.
        /// </summary>
        public enum CalibratorState
        {
            Inactive,
            Calibrating,
            Calibrated
        }

        private void Calibrate(ref UnifiedTrackingData input, float calibrationWeight)
        {
            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
            {
                // Calibrator
                if (calibrationWeight > 0.0f)
                {
                    // Detecting if a parameter exceeds the 1.0f float threshold, then cap it with it's inverse; making sure to not fully enforce the calibration value immediately with a buffer.
                    if (input.Shapes[i].Weight * MutationData.ShapeMutations[i].CalibrationMult > 1.0f)
                        MutationData.ShapeMutations[i].CalibrationMult = SimpleLerp(1.0f / input.Shapes[i].Weight, MutationData.ShapeMutations[i].CalibrationMult, calibrationWeight);
                }

                // Normalizer (after calibration/loaded calibration)
                input.Shapes[i].Weight = input.Shapes[i].Weight * MutationData.ShapeMutations[i].CalibrationMult;
            }
        }

        private void ApplyCalibrator(ref UnifiedTrackingData input)
        {
            /* Example of continuous calibration to the UnifiedTrackingData.LatestExpressionData within the mutator:
             * This will be the first step to run so apply any sort of calibration to normalize the range of the base parameter between 0.0f -> 1.0f.
             * This also only applies a multiplier to each parameter as well, so the UnifiedTrackingData.LatestExpressionData passed through here will have a consistent UnifiedTrackingData.MutatedExpressionData.
             */

            switch (CalibratorMode)
            {
                case CalibratorState.Inactive:
                    break;
                case CalibratorState.Calibrating:
                    Calibrate(ref input, CalibrationWeight);
                    break;
                case CalibratorState.Calibrated:
                    Calibrate(ref input, 0.0f);
                    break;
                default:
                    throw new ArgumentException("Calibration state not found.");
            }
        }

        private void ApplySmoothing(ref UnifiedTrackingData input)
        {
            // Example of applying smoothing to the data within the mutator:
            if (SmoothingMode)
            {
                for (int i = 0; i < input.Shapes.Length; i++)
                    input.Shapes[i].Weight =
                        SimpleLerp(
                            input.Shapes[i].Weight,
                            TrackingDataBuffer.Shapes[i].Weight,
                            MutationData.ShapeMutations[i].SmoothnessMult
                        );

                input.Eye.Left.Openness = SimpleLerp(input.Eye.Left.Openness, TrackingDataBuffer.Eye.Left.Openness, MutationData.OpennessMutations.SmoothnessMult);
                input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, TrackingDataBuffer.Eye.Left.PupilDiameter_MM, MutationData.PupilMutations.SmoothnessMult);
                input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, TrackingDataBuffer.Eye.Left.Gaze, MutationData.GazeMutations.SmoothnessMult);

                input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, TrackingDataBuffer.Eye.Right.Openness, MutationData.OpennessMutations.SmoothnessMult);
                input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, TrackingDataBuffer.Eye.Right.PupilDiameter_MM, MutationData.PupilMutations.SmoothnessMult);
                input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, TrackingDataBuffer.Eye.Right.Gaze, MutationData.GazeMutations.SmoothnessMult);
            }
        }

        /// <summary>
        /// Takes in the latest base expression Weight data from modules and mutates into the Weight data for output parameters.
        /// </summary>
        /// <returns> Mutated Expression Data. </returns>
        public UnifiedTrackingData MutateData(ref UnifiedTrackingData input)
        {
            ApplyCalibrator(ref input);
            ApplySmoothing(ref input);
            TrackingDataBuffer.CopyPropertiesOf(input);
            return TrackingDataBuffer;
        }

        public void SetCalibration(float setValue = 0.0f)
        {
            // Currently eye data does not get parsed by calibration.
            MutationData.PupilMutations.CalibrationMult = setValue;
            MutationData.GazeMutations.CalibrationMult = setValue;
            MutationData.OpennessMutations.CalibrationMult = setValue;

            for (int i = 0; i < MutationData.ShapeMutations.Length; i++)
                MutationData.ShapeMutations[i].CalibrationMult = setValue;
        }

        public void SetSmoothness(float setValue = 0.0f)
        {
            // Currently eye data does not get parsed by calibration.
            MutationData.PupilMutations.SmoothnessMult = setValue;
            MutationData.GazeMutations.SmoothnessMult = setValue;
            MutationData.OpennessMutations.SmoothnessMult = setValue;

            for (int i = 0; i < MutationData.ShapeMutations.Length; i++)
                MutationData.ShapeMutations[i].SmoothnessMult = setValue;
        }
    }
}
