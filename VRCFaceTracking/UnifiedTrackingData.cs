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
    public class ExpressionTrackingData
    {
        /// <summary>
        /// Images sent from all modules.
        /// </summary>
        public Image EyeImageData = new Image();
        public Image LipImageData = new Image();

        /// <summary>
        /// Latest Expression Data sent from all VRCFaceTracking modules.
        /// </summary>
        public UnifiedExpressionsData LatestData = new UnifiedExpressionsData();

        /// <summary>
        /// Internal Expression Data after being processed by VRCFaceTracking.
        /// </summary>
        // Internal VRCFT structure, used to drive the parameters internally.
        private UnifiedExpressionsData UpdatedData = new UnifiedExpressionsData();

        /// <summary>
        /// Function to read the Internal Expression Data that was processesed by VRCFaceTracking.
        /// </summary>
        /// <returns>Returns the processed Expression Data.</returns>
        public UnifiedExpressionsData ReadInternal() => UpdatedData;

        /// <summary>
        /// Updates the Internal Expression Data buffer with the Latest Expression Data
        /// </summary>
        public void UpdateData() 
        {
            UpdatedData = LatestData;
        }

        /// <summary>
        /// TODO: Resets all calibration and automated normalization done within VRCFaceTracking.
        /// </summary>
        public void ResetCalibration() { }
    }

    /// <summary>
    /// Structure that holds all data sent, used, and processed for use in VRCFaceTracking.
    /// </summary>
    public class UnifiedTrackingData
    {
        /// <summary>
        /// Version 1 (VRCFaceTracking SRanipal) of all accessible output parameters.
        /// </summary>
        /// <remarks>These parameters are going to be unsupported in the near future, and will be directly emulated by Version 2 (Unified Expressions) parameters.</remarks>
        public static readonly IParameter[] AllParameters_v1 = LipShapeMerger.AllLipParameters.ToArray();

        /// <summary>
        /// Version 2 (Unified Expression) of all accessible output parameters.
        /// </summary>
        public static readonly IParameter[] AllParameters_v2 = UnifiedExpressionMerger.ExpressionParameters.Union(EyeTrackingParams.ParameterList).ToArray();

        /// <summary>
        /// Central update action for all expression data to subscribe to.
        /// </summary>
        public static Action<ExpressionTrackingData> OnUnifiedDataUpdated;

        /// <summary>
        /// Accessible data that modules can interact with, and what VRCFaceTracking internally looks at for tracking data.
        /// </summary>
        public static ExpressionTrackingData LatestExpressionData = new ExpressionTrackingData();
    }
}