//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
        {
            public enum LipShape
            {
                None = -1,
                Jaw_Forward = 0,
                Jaw_Right = 1,
                Jaw_Left = 2,
                Jaw_Open = 3,
                Mouth_Ape_Shape = 4,
                Mouth_O_Shape = 5,
                Mouth_Pout = 6,
                Mouth_Lower_Right = 7,
                Mouth_Lower_Left = 8,
                Mouth_Smile_Right = 9,
                Mouth_Smile_Left = 10,
                Mouth_Sad_Right = 11,
                Mouth_Sad_Left = 12,
                Cheek_Puff_Right = 13,
                Cheek_Puff_Left = 14,
                Mouth_Lower_Inside = 15,
                Mouth_Upper_Inside = 16,
                Mouth_Lower_Overlay = 17,
                Mouth_Upper_Overlay = 18,
                Cheek_Suck = 19,
                Mouth_LowerRight_Down = 20,
                Mouth_LowerLeft_Down = 21,
                Mouth_UpperRight_Up = 22,
                Mouth_UpperLeft_Up = 23,
                Mouth_Philtrum_Right = 24,
                Mouth_Philtrum_Left = 25,
                Max = 27,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PredictionData
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
                public float[] blend_shape_weight;
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct LipData
            {
                public int frame;
                public int time;
                public IntPtr image;
                public PredictionData prediction_data;
            };
        }
    }
}