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
            #region EyeShape_v2
            public enum EyeShape_v2
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
                Eye_Left_Squeeze,
                Eye_Right_Squeeze,
                Max = 15,
            }

            [Serializable]
            public class EyeShapeTable_v2
            {
                public SkinnedMeshRenderer skinnedMeshRenderer;
                public EyeShape_v2[] eyeShapes;
            }
            #endregion

            [StructLayout(LayoutKind.Sequential)]
            public struct SingleEyeExpression
            {
                public float eye_wide; /*!<A value representing how open eye widely.*/
                public float eye_squeeze; /*!<A value representing how the eye is closed tightly.*/
                public float eye_frown; /*!<A value representing user's frown.*/
            };

            [StructLayout(LayoutKind.Sequential)]
            public struct EyeExpression
            {
                public SingleEyeExpression left;
                public SingleEyeExpression right;
            };

            [StructLayout(LayoutKind.Sequential)]
            /** @struct EyeData
			* A struct containing all data listed below.
			*/
            public struct EyeData_v2
            {
                /** Indicate if there is a user in front of HMD. */
                public bool no_user;
                /** The frame sequence.*/
                public int frame_sequence;
                /** The time when the frame was capturing. in millisecond.*/
                public int timestamp;
                public VerboseData verbose_data;
                public EyeExpression expression_data;
            }
        }
    }
}