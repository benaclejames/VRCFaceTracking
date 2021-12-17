//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Collections.Generic;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public static class SRanipal_Eye_v2
            {
                public const int ANIPAL_TYPE_EYE_V2 = 2;

                public const int WeightingCount = (int)EyeShape_v2.Max;
                private static Dictionary<EyeShape_v2, float> Weightings;

                static SRanipal_Eye_v2()
                {
                    Weightings = new Dictionary<EyeShape_v2, float>();
                    for (int i = 0; i < WeightingCount; ++i) Weightings.Add((EyeShape_v2)i, 0.0f);
                }
                
                /// <summary>
                /// Launches anipal's Eye Calibration feature (an overlay program).
                /// </summary>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                public static bool LaunchEyeCalibration()
                {
                    int result = SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
                    return result == (int)Error.WORK;
                }
            }
        }
    }
}