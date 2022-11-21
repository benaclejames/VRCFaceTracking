using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFaceTracking.Params
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression data into VRCFaceTracking output parameters.
    /// </summary>
    public static class UnifiedExpressionsMutator
    {
        /// <summary>
        /// Takes in the latest base expression data from modules and mutates the data for output to VRCFaceTracking output parameters.
        /// </summary>
        /// <param name="data">Latest Unified Expression Data</param>
        /// <returns>Mutated Expression Data.</returns>
        public static UnifiedExpressionsData MutateData(UnifiedExpressionsData data)
        {
            /* Run mutators to transform the base LatestExpressions (passed as data) into a modified UpdatedExpressions (what is returned after all methods run) here. 
             * The unmodified data can be pulled from UnifiedTracking.AllData.LatestExpressions if a mutator needs it, and the previously modified UpdatedExpression can be pulled from UnifiedTracking.AllData.ReadInternal() respectively. 
             */

            ApplyCalibration(ref data);
            ApplySmoothing(ref data);

            return data;
        }
        private static void ApplyCalibration(ref UnifiedExpressionsData data)
        {
            /* Example of applying calibration data to the mutator:
             * This will be the first step to run so apply any sort of calibration to normalize the range of the parameter between 0.0f - 1.0f.
             * This also only applies a multiplier to each parameter as well, so the data passed through here will have a consistent output.
             */

            Populate<float>(data.Shapes, 0.8f);

            for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
                data.Shapes[i] *= CalibratedShapeMultipliers[i];

        }

        private static void ApplySmoothing(ref UnifiedExpressionsData data)
        {
            /* Example of applying smoothing to the data within the data mutator:
             * This will be the last step to run so we will need to reference the existing ReadInternal() to have a frame of reference to apply the smoothing towards.
             * This applies a fixed amount of smoothing each DeltaT as well, so the data passed through here will have a consistent smoothness from the previous data.
             */
        }

        /// <summary>
        /// All calibration multipliers for the current tracking interface.
        /// </summary>
        /// <remarks> All data here is initialized to 0. TODO: VRCFaceTracking has a default method that will populate this. </remarks>
        private static float[] CalibratedShapeMultipliers = new float[(int)UnifiedExpressions.Max];

        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
    }
}
