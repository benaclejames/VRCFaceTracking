using System.Runtime.InteropServices;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Lip;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Models;

[StructLayout(LayoutKind.Sequential)]
public struct SubprocessData
{
    public ModuleMetadata ModuleData;

    // Mirror of params modules manipulate in UnifiedTracking
    public Image EyeImageData = new Image();
    public Image LipImageData = new Image();
    public UnifiedTrackingData Data = new();
#pragma warning disable CS0618
    public readonly Parameter[] AllParameters_v1 = LipShapeMerger.AllLipParameters.Union(EyeTrackingParams.ParameterList).ToArray();
    public readonly Parameter[] AllParameters_v2 = UnifiedExpressionsParameters.ExpressionParameters;
#pragma warning restore CS0618

    public SubprocessData()
    {

    }

}