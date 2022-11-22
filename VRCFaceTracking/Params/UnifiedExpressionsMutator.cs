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
    public static class UnifiedExpressionsMutator
    {
        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        static float SimpleLerp(float input, float previousInput, float value)
        {
            return input * (1 - value) + previousInput * value;
        }

        private static void ApplyCalibration(ref UnifiedExpressionsData data)
        {
            /* Example of continuous calibration to the data within the mutator:
             * This will be the first step to run so apply any sort of calibration to normalize the range of the parameter between 0.0f - 1.0f.
             * This also only applies a multiplier to each parameter as well, so the data passed through here will have a consistent output.
             */

            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
            {
                // Initialize a calibration value if none exists, can plant values from a VRCFT module.
                if (data.Shapes[i].calibrationMultiplier <= 0.025f)
                    data.Shapes[i].calibrationMultiplier = 999.0f;

                // Detecting if a parameter exceeds the 1.0f float threshold for output, therefore cap it with it's inverse, making sure to not fully enforce the calibration value immediately with a feedback buffer.
                if (data.Shapes[i].weight * data.Shapes[i].calibrationMultiplier > 1.0f)
                    UnifiedTracking.AllData.LatestExpressionData.Shapes[i].calibrationMultiplier = SimpleLerp(1.0f / data.Shapes[i].weight, data.Shapes[i].calibrationMultiplier, 0.75f);

                // Normalizer
                data.Shapes[i].weight *= data.Shapes[i].calibrationMultiplier;
            }

        }

        private static void ApplySmoothing(ref UnifiedExpressionsData data)
        {
            /* Example of applying smoothing to the data within the data mutator:
             * This will be the last step to run so we will need to reference the existing ReadInternal() to have a frame of reference to apply the smoothing towards.
             * This applies a fixed amount of smoothing each DeltaT as well, so the data passed through here will have a consistent smoothness from the previous data.
             */

            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
                SimpleLerp(data.Shapes[i].weight, UnifiedTracking.AllData.ReadInternal().Shapes[i].weight, 0.25f);
        }

        /// <summary>
        /// Takes in the latest base expression data from modules and mutates the data for output to VRCFaceTracking output parameters.
        /// </summary>
        /// <param name="data">Latest Unified Expression Data</param>
        /// <returns> Mutated Expression Data. </returns>
        public static UnifiedExpressionsData MutateData(UnifiedExpressionsData data)
        {
            /* Run mutators to transform the base LatestExpressions (passed as data) into a modified UpdatedExpressions (what is returned after all methods run) here. 
             * The unmodified data can be pulled from UnifiedTracking.AllData.LatestExpressions if a mutator needs it, and the previously modified UpdatedExpression can be pulled from UnifiedTracking.AllData.ReadInternal() respectively. 
             */

            ApplyCalibration(ref data);
            // ApplySmoothing(ref data);

            return data;
        }

        /// <summary>
        /// Resets internally calculated values for the tracking interface.
        /// </summary>
        public static void ResetCalibration()
        {

        }
    }
}
