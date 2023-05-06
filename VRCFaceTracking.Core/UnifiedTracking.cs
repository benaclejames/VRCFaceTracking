using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Lip;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking
{
    /// <summary>
    /// Class that contains all relevant data
    /// </summary>
    public class UnifiedTracking
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
        public static UnifiedTrackingData Data = new();

        /// <summary>
        /// Container of all features and functions that mutates the incoming expression data into output data suitable for driving Unified Expressions.
        /// </summary>
        /// <remarks> Mutates data on update. </remarks>
        public static UnifiedTrackingMutator Mutator;

#pragma warning disable CS0618
        /// <summary>
        /// Version 1 (VRCFaceTracking SRanipal) of all accessible output parameters.
        /// </summary>
        /// <remarks> These parameters are going to be undocumented in the near future and are directly emulated by Version 2 (Unified Expressions) parameters. </remarks>
        public static readonly IParameter[] AllParameters_v1 = LipShapeMerger.AllLipParameters.Union(EyeTrackingParams.ParameterList).ToArray();

        /// <summary>
        /// Version 2 (Unified Expressions) of all accessible output parameters.
        /// </summary>
        public static readonly IParameter[] AllParameters_v2 = UnifiedExpressionsMerger.ExpressionParameters;
#pragma warning restore CS0618

        /// <summary>
        /// Central update action for all expression data to subscribe to.
        /// </summary>
        public static Action<UnifiedTrackingData> OnUnifiedDataUpdated = OnUnifiedDataUpdated + (_ => { }) ?? (_ => {}); 

        /// <summary>
        /// Central update function that updates all output parameter data and pushes the latest expressions from VRCFaceTracking modules into the internal expressions buffer.
        /// </summary>
        public static void UpdateData() => OnUnifiedDataUpdated.Invoke(Mutator.MutateData(Data));
    }
}