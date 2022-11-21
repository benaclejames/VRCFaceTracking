using System;
using System.Linq;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Eye;
using VRCFaceTracking.Params.LipMerging;

namespace VRCFaceTracking
{
    /// <summary>
    /// Class that represents all Expression Tracking Data.
    /// </summary>
    public class UnifiedTrackingData
    {
        /// <summary>
        /// Image data sent from all modules. May make into an enumerable structure for add-in support for more images.
        /// </summary>
        public Image EyeImageData = new Image();
        public Image LipImageData = new Image();

        /// <summary>
        /// Latest Expression Data sent from all VRCFaceTracking modules.
        /// </summary>
        public UnifiedExpressionsData LatestExpressionData = new UnifiedExpressionsData();

        /// <summary>
        /// Internal Expression Data after being processed by VRCFaceTracking.
        /// </summary>
        private UnifiedExpressionsData UpdatedExpressionData = new UnifiedExpressionsData();

        /// <summary>
        /// Function that reads the Internal Expression Data that was processesed by VRCFaceTracking.
        /// </summary>
        /// <returns>Returns the processed Expression Data.</returns>
        public UnifiedExpressionsData ReadInternal() => UpdatedExpressionData;

        /// <summary>
        /// Updates the Internal Expression Data buffer with the Latest Expression Data.
        /// </summary>
        public void UpdateData() => UpdatedExpressionData = UnifiedExpressionsMutator.MutateData(LatestExpressionData);

        /// <summary>
        /// TODO: Resets all calibration and automated normalization done within UnifiedExpressionsMutator.
        /// </summary>
        public void ResetCalibration() { }
    }

    /// <summary>
    /// Structure that holds everything that VRCFaceTracking needs to take tracking data from modules to a formatted output.
    /// </summary>
    public static class UnifiedTracking
    {
        /// <summary>
        /// Version 1 (VRCFaceTracking SRanipal) of all accessible output parameters.
        /// </summary>
        /// <remarks>These parameters are going to be unsupported in the near future, and will be directly emulated by Version 2 (Unified Expressions) parameters.</remarks>
        public static readonly IParameter[] AllParameters_v1 = LipShapeMerger.AllLipParameters;

        /// <summary>
        /// Version 2 (Unified Expression) of all accessible output parameters.
        /// </summary>
        public static readonly IParameter[] AllParameters_v2 = UnifiedExpressionMerger.ExpressionParameters.Union(EyeTrackingParams.ParameterList).ToArray();

        /// <summary>
        /// Central update action for all expression data to subscribe to.
        /// </summary>
        public static Action<UnifiedExpressionsData> OnUnifiedDataUpdated;

        /// <summary>
        /// All data that modules can interact with, and what VRCFaceTracking internally looks at for getting data.
        /// </summary>
        public static UnifiedTrackingData AllData = new UnifiedTrackingData();
    }
}