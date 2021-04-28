//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Runtime.InteropServices;

namespace ViveSR
{
    namespace anipal
    {
        public class SRanipal_API
        {
            /// <summary>
            /// Invokes an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <param name="config">Indicates the resulting ViveSR.Error status of this method.</returns>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error Initial(int anipalType, IntPtr config);

            /// <summary>
            /// Terminates an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error Release(int anipalType);

            /// <summary>
            /// Gets the status of an anipal module.
            /// </summary>
            /// <param name="anipalType">The index of an anipal module.</param>
            /// <param name="status">The status of an anipal module.</param>
            /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
            [DllImport("SRanipal")]
            public static extern Error GetStatus(int anipalType, out AnipalStatus status);

        }
    }
}
