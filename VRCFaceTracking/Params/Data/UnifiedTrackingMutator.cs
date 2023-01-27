using System;
using System.Drawing.Drawing2D;
using System.Windows.Markup;
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
            public float CalibrationMult; // The amount to multiply this parameter to normalize between 0-1.
            //public float SigmoidMult; // How much should this parameter be affected by the sigmoid function. This makes the parameter act more like a toggle.
            //public float LogitMult; // How much should this parameter be affected by the logit (inverse of sigmoid) function. This makes the parameter act more within the normalized range.
            public float SmoothnessMult; // How much should this parameter be affected by the smoothing function.
        }

        public class MutationData
        {
            public static Mutation[] ShapeMutations = new Mutation[(int)UnifiedExpressions.Max + 1];
            public static Mutation GazeMutations, OpennessMutations, PupilMutations = new Mutation();
        }

        private static UnifiedTrackingData trackingDataBuffer = new UnifiedTrackingData();
        public static MutationData mutationData = new MutationData();

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

        static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

        private void Calibrate(ref UnifiedTrackingData inputData, float calibrationWeight)
        {
            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
            {
                if (calibrationWeight > 0.0f && inputData.Shapes[i].Weight * MutationData.ShapeMutations[i].CalibrationMult > 1.0f) // Calibrator
                        MutationData.ShapeMutations[i].CalibrationMult = SimpleLerp(1.0f / inputData.Shapes[i].Weight, MutationData.ShapeMutations[i].CalibrationMult, calibrationWeight);

                inputData.Shapes[i].Weight = Math.Min(1, (inputData.Shapes[i].Weight * MutationData.ShapeMutations[i].CalibrationMult));
            }
        }

        private void ApplyCalibrator(ref UnifiedTrackingData inputData)
            => Calibrate(ref inputData, CalibratorMode == CalibratorState.Calibrating ? CalibrationWeight : 0.0f);

        private void ApplySmoothing(ref UnifiedTrackingData input)
        {
            // Example of applying smoothing to the data within the mutator:
            if (SmoothingMode)
            {
                for (int i = 0; i < input.Shapes.Length; i++)
                    input.Shapes[i].Weight =
                        SimpleLerp(
                            input.Shapes[i].Weight,
                            trackingDataBuffer.Shapes[i].Weight,
                            MutationData.ShapeMutations[i].SmoothnessMult
                        );

                input.Eye.Left.Openness = SimpleLerp(input.Eye.Left.Openness, trackingDataBuffer.Eye.Left.Openness, MutationData.OpennessMutations.SmoothnessMult);
                input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, trackingDataBuffer.Eye.Left.PupilDiameter_MM, MutationData.PupilMutations.SmoothnessMult);
                input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, trackingDataBuffer.Eye.Left.Gaze, MutationData.GazeMutations.SmoothnessMult);

                input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, trackingDataBuffer.Eye.Right.Openness, MutationData.OpennessMutations.SmoothnessMult);
                input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, trackingDataBuffer.Eye.Right.PupilDiameter_MM, MutationData.PupilMutations.SmoothnessMult);
                input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, trackingDataBuffer.Eye.Right.Gaze, MutationData.GazeMutations.SmoothnessMult);
            }
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
