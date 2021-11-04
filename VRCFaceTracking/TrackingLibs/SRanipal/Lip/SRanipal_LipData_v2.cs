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
                JawRight = 0, // +JawX
                JawLeft = 1, // -JawX
                JawForward = 2,
                JawOpen = 3,
                MouthApeShape = 4,
                MouthUpperRight = 5, // +MouthUpper
                MouthUpperLeft = 6, // -MouthUpper
                MouthLowerRight = 7, // +MouthLower
                MouthLowerLeft = 8, // -MouthLower
                MouthUpperOverturn = 9,
                MouthLowerOverturn = 10,
                MouthPout = 11,
                MouthSmileRight = 12, // +SmileSadRight
                MouthSmileLeft = 13, // +SmileSadLeft
                MouthSadRight = 14, // -SmileSadRight
                MouthSadLeft = 15, // -SmileSadLeft
                CheekPuffRight = 16,
                CheekPuffLeft = 17,
                CheekSuck = 18,
                MouthUpperUpRight = 19,
                MouthUpperUpLeft = 20,
                MouthLowerDownRight = 21,
                MouthLowerDownLeft = 22,
                MouthUpperInside = 23,
                MouthLowerInside = 24,
                MouthLowerOverlay = 25,
                TongueLongStep1 = 26,
                TongueLongStep2 = 32,
                TongueDown = 30, // -TongueY
                TongueUp = 29, // +TongueY
                TongueRight = 28, // +TongueX
                TongueLeft = 27, // -TongueX
                TongueRoll = 31,
                TongueUpLeftMorph = 34,
                TongueUpRightMorph = 33,
                TongueDownLeftMorph = 36,
                TongueDownRightMorph = 35,
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