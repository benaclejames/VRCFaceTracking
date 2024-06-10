using System.Runtime.InteropServices;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Lip;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Models;

public partial class SubprocessData
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SubprocessData_Protocol_Data
    {
        #region ModuleData

        public string ModuleData_Name;
        public bool ModuleData_Active;
        public bool ModuleData_HasEye;
        public bool ModuleData_HasExpression;

        #endregion

        #region ParametersData

        public float EyeSquintRight;

        #endregion
    }

    public bool IsDataTrustworthy => _isDataTrustworthy;
    
    public ModuleMetadata ModuleData = new();

    // Mirror of params modules manipulate in UnifiedTracking
    public Image EyeImageData               = null;
    public Image LipImageData               = null;
    public UnifiedTrackingData Data         = null;
#pragma warning disable CS0618
    public Parameter[] AllParameters_v1     = LipShapeMerger.AllLipParameters.Union(EyeTrackingParams.ParameterList).ToArray();
    public Parameter[] AllParameters_v2     = UnifiedExpressionsParameters.ExpressionParameters;
#pragma warning restore CS0618

    private bool _isDataTrustworthy;

    public SubprocessData()
    {
        // The data is not to be trusted until the module returns any data through the IPC protocol
        // We only deem the module to be trustworthy IF and ONLY IF we received:
        //  - The stream images
        //  - Initialisation succeeded successfully
        _isDataTrustworthy = false;

    }
}