using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Types;
using RealUnifiedTracking = VRCFaceTracking.UnifiedTracking;
using VRCFTParameter = VRCFaceTracking.Core.Params.Parameter;

namespace VRCFaceTracking.ModuleProcess;

// Proxy of Unified tracking class
// Handles syncing our Subprocess data with the "fake" UnifiedTracking implementation we expose to modules
public static class UnifiedTrackingProxy {

    private static void UpdateImage(ref Image destination, ref Image source)
    {
		destination.SupportsImage   = source.SupportsImage;
		destination.ImageSize       = source.ImageSize;
		destination.ImageData       = source.ImageData;
	}

	private static void CopyArrayFast(ref VRCFTParameter[] destination, VRCFTParameter[] source) {

	}

    public static void UpdateSharedData(ref SubprocessData subprocessData)
    {
		UpdateImage(ref subprocessData.EyeImageData, ref RealUnifiedTracking.EyeImageData);
		UpdateImage(ref subprocessData.LipImageData, ref RealUnifiedTracking.LipImageData);

		CopyArrayFast(ref subprocessData.AllParameters_v1, RealUnifiedTracking.AllParameters_v1);
		CopyArrayFast(ref subprocessData.AllParameters_v2, RealUnifiedTracking.AllParameters_v2);
    }
}