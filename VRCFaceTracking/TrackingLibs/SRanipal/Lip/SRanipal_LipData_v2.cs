//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
        {
            // LipShape_v2 => LipShape_v3 moved to LipShapes

            [StructLayout(LayoutKind.Sequential)]
            public struct PredictionData_v2
            {
                 public unsafe fixed float blend_shape_weight[60];
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct LipData_v2
            {
                public int frame;
                public int time;
                public IntPtr image;
                public PredictionData_v2 prediction_data;
            };
        }
    }
}