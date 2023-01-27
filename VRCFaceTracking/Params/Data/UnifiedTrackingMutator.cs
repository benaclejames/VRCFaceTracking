using System;
using System.Windows.Markup;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
    /// </summary>
    public class UnifiedTrackingMutator
    {
        public CalibratorState calibratorMode = CalibratorState.Inactive;
        public float calibrationWeight;
        public bool smoothingMode = false;

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
                // Calibrator
                if (calibrationWeight > 0.0f)
                {
                    // Detecting if a parameter exceeds the 1.0f float threshold, then cap it with it's inverse; making sure to not fully enforce the calibration value immediately with a buffer.
                    if (inputData.Shapes[i].Weight * inputData.Shapes[i].CalibrationMult > 1.0f)
                        UnifiedTracking.Data.Shapes[i].CalibrationMult = SimpleLerp(1.0f / inputData.Shapes[i].Weight, inputData.Shapes[i].CalibrationMult, calibrationWeight);
                }

                // Normalizer (after calibration/loaded calibration)
                UnifiedTracking.Data.Shapes[i].AdjustedWeight = UnifiedTracking.Data.Shapes[i].Weight * UnifiedTracking.Data.Shapes[i].CalibrationMult;
            }
        }

        /* Example of continuous calibration to the UnifiedTrackingData.LatestExpressionData within the mutator:
        * This will be the first step to run so apply any sort of calibration to normalize the range of the base parameter between 0.0f -> 1.0f.
        * This also only applies a multiplier to each parameter as well, so the UnifiedTrackingData.LatestExpressionData passed through here will have a consistent UnifiedTrackingData.MutatedExpressionData.
        */
        private void ApplyCalibrator(ref UnifiedTrackingData inputData)
            => Calibrate(ref inputData, calibratorMode == CalibratorState.Calibrating ? calibrationWeight : 0.0f);
        
        private void ApplySmoothing(ref UnifiedTrackingData data)
        {
            // Example of applying smoothing to the data within the mutator:
            if (smoothingMode)
            {
                for (int i = 0; i < data.Shapes.Length; i++)
                    data.Shapes[i].AdjustedWeight =
                        SimpleLerp(
                            data.Shapes[i].Weight,
                            data.Shapes[i].AdjustedWeight,
                            data.Shapes[i].SmoothnessMult
                        );

                data.AdjustedEye.Left.Openness = SimpleLerp(data.Eye.Left.Openness, data.AdjustedEye.Left.Openness, data.Eye.OpennessSmoothness);
                data.AdjustedEye.Left.PupilDiameter_MM = SimpleLerp(data.Eye.Left.PupilDiameter_MM, data.AdjustedEye.Left.PupilDiameter_MM, data.Eye.GazeSmoothness);
                data.AdjustedEye.Left.Gaze = SimpleLerp(data.Eye.Left.Gaze, data.AdjustedEye.Left.Gaze, data.Eye.PupilDiameterSmoothness);

                data.AdjustedEye.Right.Openness = SimpleLerp(data.Eye.Right.Openness, data.AdjustedEye.Right.Openness, data.Eye.OpennessSmoothness);
                data.AdjustedEye.Right.PupilDiameter_MM = SimpleLerp(data.Eye.Right.PupilDiameter_MM, data.AdjustedEye.Right.PupilDiameter_MM, data.Eye.GazeSmoothness);
                data.AdjustedEye.Right.Gaze = SimpleLerp(data.Eye.Right.Gaze, data.AdjustedEye.Right.Gaze, data.Eye.PupilDiameterSmoothness);
            }
        }

        /// <summary>
        /// Takes in the latest base expression Weight data from modules and mutates into the AdjustedWeight data for output parameters.
        /// </summary>
        /// <returns> Mutated Expression Data. </returns>
        public void MutateData(UnifiedTrackingData inputData)
        {
            ApplyCalibrator(ref inputData);
            ApplySmoothing(ref inputData);
        }
    }
}
