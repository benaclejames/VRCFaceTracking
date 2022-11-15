namespace VRCFaceTracking.Params.Lip
{
    // TODO: Create expressions map to relate legacy shapes to new ones
    public enum UnifiedExpression
    {
        #region Eye Gaze Expressions

        // These are currently unused in the Lips expressions, and used in Eye Expressions.

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

        EyeSquintLeft, // Eye Squeeze from SRanipal (excluding brow movements), with additional consideration for Quest Pro's squint tracking
        EyeSquintRight,
        EyeClosedLeft, // EyeBlink
        EyeClosedRight,
        EyeWideLeft,
        EyeWideRight,
        EyeDilationLeft,
        EyeDilationRight,
        EyeConstrictLeft,
        EyeConstrictRight,

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
        CheekSquintRight,
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

        MouthApeShape, // Map like SRanipal's MouthApeShape

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
        // MouthShrugLower, // Duplicate shape to MouthRaiserLower
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
        TongueRoll

        #endregion
    }
}