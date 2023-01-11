using System;
using System.Linq;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Eye;
using VRCFaceTracking.Params.LipMerging;

namespace VRCFaceTracking
{
    /// <summary>
    /// Class that contains all relevant data
    /// </summary>
    public static class UnifiedTracking
    {
        /// <summary>
        /// Eye image data sent from the loaded eye module.
        /// </summary>
        public static Image EyeImageData = new Image();

        /// <summary>
        /// Lip / Expression image data sent from the loaded expressions module.
        /// </summary>
        public static Image LipImageData = new Image();

        /// <summary>
        /// Latest Expression Data accessible and sent by all VRCFaceTracking modules.
        /// </summary>
        public static UnifiedTrackingData Data = new UnifiedTrackingData();

        /// <summary>
        /// Container of all features and functions that mutates the incoming expression data into output data suitable for driving Unified Expressions.
        /// </summary>
        /// <remarks> Mutates data on update. </remarks>
        public static UnifiedTrackingMutator Mutator = new UnifiedTrackingMutator();

        /// <summary>
        /// Version 1 (VRCFaceTracking SRanipal) of all accessible output parameters.
        /// </summary>
        /// <remarks> These parameters are going to be undocumented in the near future and are directly emulated by Version 2 (Unified Expressions) parameters. </remarks>
        public static readonly IParameter[] AllParameters_v1 = LipShapeMerger.AllLipParameters.Union(EyeTrackingParams.ParameterList).ToArray();

        /// <summary>
        /// Version 2 (Unified Expressions) of all accessible output parameters.
        /// </summary>
        public static readonly IParameter[] AllParameters_v2 = UnifiedExpressionsMerger.ExpressionParameters;

        /// <summary>
        /// Central update action for all expression data to subscribe to.
        /// </summary>
        public static Action<UnifiedTrackingData> OnUnifiedDataUpdated;

        /// <summary>
        /// Central update function that updates all output parameter data and pushes the latest expressions from VRCFaceTracking modules into the internal expressions buffer.
        /// </summary>
        public static void UpdateData()
        {
            // 'Push' model; Updates will push the latest data buffer from modules and mutates the output for parameters.
            // All data pertaining to how the mutator transforms the latest data into mutated data is contained within Data itself.
            Mutator.MutateData(Data);
            OnUnifiedDataUpdated.Invoke(Data);
        }
    }
}