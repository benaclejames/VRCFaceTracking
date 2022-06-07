//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
        {
            public class SRanipal_Lip_v2
            {
                public const int ANIPAL_TYPE_LIP_V2 = 3;

                public const int ImageWidth = 800, ImageHeight = 400, ImageChannel = 1;
                public const int WeightingCount = 37;
                private static Error LastUpdateResult = Error.FAILED;
                public static LipData_v2 LipData;
                private static Dictionary<LipShape_v2, float> Weightings;

                static SRanipal_Lip_v2()
                {
                    LipData.image = Marshal.AllocCoTaskMem(ImageWidth * ImageHeight * ImageChannel);
                    Weightings = new Dictionary<LipShape_v2, float>();
                    for (int i = 0; i < WeightingCount; ++i) Weightings.Add((LipShape_v2)i, 0.0f);
                }

                private static unsafe bool UpdateData()
                {
                    LastUpdateResult = SRanipal_Lip_API.GetLipData_v2(ref LipData);
                    if (LastUpdateResult == Error.WORK)
                    {
                        for (int i = 0; i < WeightingCount; ++i)
                        {
                            Weightings[(LipShape_v2)i] = LipData.prediction_data.blend_shape_weight[i];
                        }
                    }
                    return LastUpdateResult == Error.WORK;
                }

                /// <summary>
                /// Gets weighting values from anipal's Lip module.
                /// </summary>
                /// <param name="shapes">Weighting values obtained from anipal's Lip module.</param>
                /// <returns>Indicates whether the values received are new.</returns>
                public static bool GetLipWeightingsAndImage(out Dictionary<LipShape_v2, float> shapes, out IntPtr image)
                {
                    bool update = UpdateData();
                    shapes = Weightings;
                    image = LipData.image;
                    return update;
                }
            }
        }
    }
}

