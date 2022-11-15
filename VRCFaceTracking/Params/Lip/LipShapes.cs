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

        EyeSquintLeft,
        EyeSquintRight,
        EyeClosedLeft,
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

        CheekSquintLeft,
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

        MouthApeShape,

        #endregion

        #region Lip Expressions

        LipSuckTopRight,
        LipSuckTopLeft,
        LipSuckBottomRight,
        LipSuckBottomLeft,
        LipFunnelTopRight,
        LipFunnelTopLeft,
        LipFunnelBottomRight,
        LipFunnelBottomLeft,
        LipPuckerLeft,
        LipPuckerRight,
        MouthUpperUpLeft,
        MouthUpperUpRight,
        MouthLowerDownLeft,
        MouthLowerDownRight,
        MouthUpperLeft, //Orig MouthTopLeft
        MouthUpperRight, //Orig MouthTopRight
        MouthLowerLeft, //Orig MouthBottomLeft
        MouthLowerRight, //Orig MouthBottomRight
        MouthSmileLeft,
        MouthSmileRight,
        MouthFrownLeft,
        MouthFrownRight,
        MouthDimpleLeft,
        MouthDimpleRight,
        MouthRaiserUpper,
        MouthRaiserLower,
        // MouthShrugLower, // Duplicate shape to MouthRaiserLower
        MouthPressLeft,
        MouthPressRight,
        MouthTightenerLeft,
        MouthTightenerRight,
        MouthStretchLeft,
        MouthStretchRight,

        #endregion

        #region Tongue Expressions

        TongueOut,
        TongueDown,
        TongueUp,
        TongueLeft,
        TongueRight,
        TongueRoll

        #endregion
    }
}