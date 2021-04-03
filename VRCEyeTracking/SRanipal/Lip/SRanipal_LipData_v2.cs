//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
        {
            public enum LipShape_v2
            {
                None = -1,
                Jaw_Right = 0,
                Jaw_Left = 1,
                Jaw_Forward = 2,
                Jaw_Open = 3,
                Mouth_Ape_Shape = 4,
                Mouth_Upper_Right = 5,
                Mouth_Upper_Left = 6,
                Mouth_Lower_Right = 7,
                Mouth_Lower_Left = 8,
                Mouth_Upper_Overturn = 9,
                Mouth_Lower_Overturn = 10,
                Mouth_Pout = 11,
                Mouth_Smile_Right = 12,
                Mouth_Smile_Left = 13,
                Mouth_Sad_Right = 14,
                Mouth_Sad_Left = 15,
                Cheek_Puff_Right = 16,
                Cheek_Puff_Left = 17,
                Cheek_Suck = 18,
                Mouth_Upper_UpRight = 19,
                Mouth_Upper_UpLeft = 20,
                Mouth_Lower_DownRight = 21,
                Mouth_Lower_DownLeft = 22,
                Mouth_Upper_Inside = 23,
                Mouth_Lower_Inside = 24,
                Mouth_Lower_Overlay = 25,
                Tongue_LongStep1 = 26,
                Tongue_LongStep2 = 32,
                Tongue_Down = 30,
                Tongue_Up = 29,
                Tongue_Right = 28,
                Tongue_Left = 27,
                Tongue_Roll = 31,
                Tongue_UpLeft_Morph = 34,
                Tongue_UpRight_Morph = 33,
                Tongue_DownLeft_Morph = 36,
                Tongue_DownRight_Morph = 35,
                Max = 37,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PredictionData_v2
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
                public float[] blend_shape_weight;
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