//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            #region EyeShape_v1
            public enum EyeShape
            {
                None = -1,
                Eye_Left_Blink = 0,
                Eye_Left_Wide,
                Eye_Left_Right,
                Eye_Left_Left,
                Eye_Left_Up,
                Eye_Left_Down,
                Eye_Right_Blink = 6,
                Eye_Right_Wide,
                Eye_Right_Right,
                Eye_Right_Left,
                Eye_Right_Up,
                Eye_Right_Down,
                Eye_Frown = 12,
                Max = 13,
            }

            [Serializable]
            public class EyeShapeTable
            {
                public SkinnedMeshRenderer skinnedMeshRenderer;
                public EyeShape[] eyeShapes;
            }
            #endregion

            [StructLayout(LayoutKind.Sequential)]
            /** @struct EyeData
			* A struct containing all data listed below.
			*/
            public struct EyeData
            {
                public bool no_user;
                /** The frame sequence.*/
                public int frame_sequence;
                /** The time when the frame was capturing. in millisecond.*/
                public int timestamp;
                public VerboseData verbose_data;
            }
        }
    }
}