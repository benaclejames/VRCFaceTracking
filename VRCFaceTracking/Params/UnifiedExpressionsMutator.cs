using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Params;

namespace VRCFaceTracking
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression data into VRCFaceTracking output parameters.
    /// </summary>
    public class UnifiedExpressionsMutator
    {
        public CalibratorState calibratorMode = CalibratorState.Calibrating;
        public bool applySmoothing = false;

        static float SimpleLerp(float input, float previousInput, float value)
        {
            return input * (1 - value) + previousInput * value;
        }

        private void Calibrate(ref UnifiedExpressionsData data, float calibrationWeight)
        {
            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
            {
                // Calibrator
                if (calibrationWeight != 0.0f)
                {
                    // Initialize a calibration value if none exists, can plant values from a VRCFT module.
                    if (data.Shapes[i].CalibrationMultiplier <= 0.025f)
                        data.Shapes[i].CalibrationMultiplier = 999.0f;

                    // Detecting if a parameter exceeds the 1.0f float threshold for output, therefore cap it with it's inverse, making sure to not fully enforce the calibration value immediately with a feedback buffer.
                    if (data.Shapes[i].Weight * data.Shapes[i].CalibrationMultiplier > 1.0f)
                        UnifiedTracking.AllData.LatestExpressionData.Shapes[i].CalibrationMultiplier = SimpleLerp(1.0f / data.Shapes[i].Weight, data.Shapes[i].CalibrationMultiplier, calibrationWeight);
                }

                // Normalizer (after calibration/loaded calibration)
                data.Shapes[i].Weight *= data.Shapes[i].CalibrationMultiplier;
            }
        }

        private void ApplyCalibrator(ref UnifiedExpressionsData data)
        {
            /* Example of continuous calibration to the data within the mutator:
             * This will be the first step to run so apply any sort of calibration to normalize the range of the parameter between 0.0f - 1.0f.
             * This also only applies a multiplier to each parameter as well, so the data passed through here will have a consistent output.
             */

            switch (calibratorMode)
            {
                case CalibratorState.Inactive:
                    break;
                case CalibratorState.Calibrating:
                    Calibrate(ref data, 0.75f);
                    break;
                case CalibratorState.FineTuning:
                    Calibrate(ref data, 0.2f);
                    break;
                case CalibratorState.Calibrated:
                    Calibrate(ref data, 0.0f);
                    break;
                default:
                    throw new ArgumentException("Calibration state not found.");
            }
        }

        private void ApplySmoothing(ref UnifiedExpressionsData data)
        {
            /* Example of applying smoothing to the data within the data mutator:
             * This will be the last step to run so we will need to reference the existing ReadInternal() to have a frame of reference to apply the smoothing towards.
             * This applies a fixed amount of smoothing each DeltaT as well, so the data passed through here will have a consistent smoothness from the previous data.
             */

            if (applySmoothing)
                for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
                    SimpleLerp(data.Shapes[i].Weight, UnifiedTracking.AllData.ReadInternal().Shapes[i].Weight, data.Shapes[i].SmoothingMultiplier);
        }

        /// <summary>
        /// Takes in the latest base expression data from modules and mutates the data for output to VRCFaceTracking output parameters.
        /// </summary>
        /// <param name="data">Latest Unified Expression Data</param>
        /// <returns> Mutated Expression Data. </returns>
        public UnifiedExpressionsData MutateData(UnifiedExpressionsData data)
        {
            /* Run mutators to transform the base LatestExpressions (passed as data) into a modified UpdatedExpressions (what is returned after all methods run) here. 
             * The unmodified data can be pulled from UnifiedTracking.AllData.LatestExpressions if a mutator needs it, and the previously modified UpdatedExpression can be pulled from UnifiedTracking.AllData.ReadInternal() respectively. 
             */

            ApplyCalibrator(ref data);
            //ApplySmoothing(ref data);

            return data;
        }

        /// <summary>
        /// Resets internally calculated values for the tracking interface.
        /// </summary>
        public void ResetCalibration()
        {

        }

        public enum CalibratorState
        {
            Inactive,
            Calibrating,
            FineTuning,
            Calibrated
        }
    }
}
