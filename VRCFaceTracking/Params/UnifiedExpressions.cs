namespace VRCFaceTracking.Params
{
    // TODO: Create expressions map to relate legacy shapes to new ones

    /// <summary> 
    /// Represents the type of Shape data being sent by UnifiedExpressionData, in the form of enumerated shapes.
    /// </summary>
    public enum UnifiedExpressions
    {
        #region Eye Gaze Expressions

        // These are currently unused in the Lips expressions, and used in UnifiedEye.

        // EyeLookOutRight,
        // EyeLookInRight,
        // EyeLookUpRight,
        // EyeLookDownRight,
        // EyeLookOutLeft,
        // EyeLookInLeft,
        // EyeLookUpLeft,
        // EyeLookDownLeft,

        #endregion

        #region Eye Expressions

        // Biometrically accurate data. Included with UnifiedEye
        //EyeClosedLeft, // EyeBlink
        //EyeClosedRight,
        //EyeDilationLeft,
        //EyeDilationRight,
        //EyeConstrictLeft,
        //EyeConstrictRight,

        EyeSquintLeft, // Eye Squeeze from SRanipal (excluding brow movements), with additional consideration for Quest Pro's squint tracking
        EyeSquintRight,
        EyeWideLeft, // Eye Squeeze from SRanipal (excluding brow movements), with additional consideration for Quest Pro's squint tracking
        EyeWideRight,

        #endregion

        #region EyeBrow Expressions

        BrowInnerDownLeft,
        BrowInnerDownRight,
        BrowOuterDownLeft,
        BrowOuterDownRight,
        BrowInnerUpLeft,
        BrowInnerUpRight,
        BrowOuterUpLeft,
        BrowOuterUpRight,

        #endregion

        #region Nose Expressions

        NoseSneerLeft,
        NoseSneerRight,

        #endregion

        #region Cheek Expressions

        CheekSquintLeft, // Raises cheeks exclusively
        CheekSquintRight, // Raises cheeks exclusively
        CheekPuffLeft,
        CheekPuffRight,
        CheekSuckLeft,
        CheekSuckRight,

        #endregion

        #region Jaw Exclusive Expressions

        JawOpen,
        JawLeft,
        JawRight,
        JawForward,

        MouthClosed, // Map like SRanipal's MouthApeShape

        #endregion

        #region Lip Expressions

        LipSuckTopRight, // Mouth Upper Inside uncombined
        LipSuckTopLeft,
        LipSuckBottomRight, // Mouth Lower Inside uncombined
        LipSuckBottomLeft,
        LipFunnelTopRight, // Mouth makes a funnel shape (unlike SRanipal's Mouth Upper/Lower Overturn, it also opens the mouth into a funnel)
        LipFunnelTopLeft,
        LipFunnelBottomRight,
        LipFunnelBottomLeft,
        LipPuckerLeft, // Lip Pout uncombined
        LipPuckerRight,
        MouthUpperUpLeft,
        MouthUpperUpRight,
        MouthLowerDownLeft,
        MouthLowerDownRight,
        MouthTopLeft, // Mouth Upper Left
        MouthTopRight, // Mouth Upper Right
        MouthBottomLeft, // Mouth Lower Left
        MouthBottomRight, // Mouth Lower Right
        MouthSmileLeft,
        MouthSmileRight,
        MouthFrownLeft,
        MouthFrownRight,
        MouthDimpleLeft,
        MouthDimpleRight,
        MouthRaiserUpper, // Raises the upper lip of the mouth
        MouthRaiserLower, // Raises the lower lip of the mouth (and may also track Mouth Lower Overlay as well)
        //MouthShrugLower, // Duplicate shape to MouthRaiserLower
        MouthPressLeft, // Squeezes the lips together
        MouthPressRight, // Squeezes the lips together
        MouthTightenerLeft,
        MouthTightenerRight,
        MouthStretchLeft,
        MouthStretchRight,

        #endregion

        #region Tongue Expressions

        TongueOut, // Combined LongStep1 and LongStep2 into one shape, as it can be emulated in-animation
        TongueDown,
        TongueUp,
        TongueLeft,
        TongueRight,
        TongueRoll,

        #endregion

        Max
    }
    /// <summary> 
    /// Represents the type of Legacy Shape data being sent by UnifiedExpressionData, in the form of enumerated SRanipal shapes.
    /// </summary>
    /// <remarks>
    /// This enum is not intended to be used directly by modules in the final iteration, and instead will be used as a compatibility layer for making the new Unified system backwards compatible with older VRCFT avatars.
    /// </remarks>
    public enum SRanipal_LipShape_v2
    {
        //None = -1,
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
}
