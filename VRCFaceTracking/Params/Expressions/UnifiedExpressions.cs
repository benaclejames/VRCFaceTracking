namespace VRCFaceTracking.Params
{
    // TODO: Create expressions map to relate legacy shapes to new ones

    /// <summary> 
    /// Represents the type of Shape data being sent by UnifiedExpressionData, in the form of enumerated shapes.
    /// </summary>
    public enum UnifiedExpressions
    {
        #region Eye Gaze Expressions

        // These are currently unused for expressions and used in the UnifiedEye structure.
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

        // 'Biometrically' accurate data that is included with UnifiedEye
        //EyeClosedRight, // Closes the right eyelid
        //EyeClosedLeft, // Closes the left eyelid
        //EyeDilationRight, // Dilates the right eye's pupil
        //EyeDilationLeft, // Dilates the left eye's pupil
        //EyeConstrictRight, // Constricts the right eye's pupil
        //EyeConstrictLeft, // Constricts the left eye's pupil

        EyeSquintRight, // Squeezes the right eye socket muscles, causing the lower eyelid to constrict a little bit as well.
        EyeSquintLeft, // Squeezes the left eye socket muscles, causing the lower eyelid to constrict a little bit as well.
        EyeWideRight, // Right eyelid widens beyond the eyelid's relaxed position.
        EyeWideLeft, // Left eyelid widens beyond the eyelid's relaxed position.

        #endregion

        #region EyeBrow Expressions

        BrowInnerDownRight, // Inner right eyebrow pushes downward.
        BrowInnerDownLeft, // Inner left eyebrow pushes downward.
        BrowOuterDownRight, // Outer right eyebrow pushes downward.
        BrowOuterDownLeft, // Outer Left eyebrow pushes downward.
        BrowInnerUpRight, // Inner right eyebrow pushes upward.
        BrowInnerUpLeft, // Inner left eyebrow pushes upward.
        BrowOuterUpRight, // Outer right eyebrow pushes upward.
        BrowOuterUpLeft, // Outer left eyebrow pushes upward.

        #endregion

        #region Nose Expressions

        NoseDilationRight, // Right side nose's canal opens up.
        NoseDilationLeft, // Left side nose's canal opens up.

        NoseWrinkleRight, // Outer part of the right side nose pulls upward.
        NoseWrinkleLeft, // Outer part of the right side nose pulls upward.

        #endregion

        #region Cheek Expressions

        CheekSquintRight, // Raises the right side cheek.
        CheekSquintLeft, // Raises the left side cheek.
        CheekPuffRight, // Puffs the right side cheek.
        CheekPuffLeft, // Puffs the left side cheek.
        CheekSuckRight, // Sucks in the right side cheek.
        CheekSuckLeft, // Sucks in the left side cheek.

        #endregion

        #region Jaw Exclusive Expressions

        JawOpen, // Opens the jawbone.
        JawRight, // Pushes the jawbone right.
        JawLeft, // Pushes the jawbone left.
        JawForward, // Pushes the jawbone forward.

        MouthClosed, // Closes the mouth relative to JawOpen.

        #endregion

        #region Lip Expressions

        LipSuckUpperRight, // Upper right part of the lip gets tucked inside the mouth.
        LipSuckUpperLeft, // Upper left part of the lip gets tucked inside the mouth.
        LipSuckLowerRight, // Lower right part of the lip gets tucked inside the mouth.
        LipSuckLowerLeft, // Lower left part of the lip gets tucked inside the mouth.

        LipFunnelUpperRight, // Upper right part of the lip pushes outward into a funnel shape.
        LipFunnelUpperLeft, // Upper left part of the lip pushes outward into a funnel shape.
        LipFunnelLowerRight, // Lower right part of the lip pushes outward into a funnel shape.
        LipFunnelLowerLeft, // Lower left part of the lip pushes outward into a funnel shape.

        LipPuckerRight, // Right part of the lip pinches inward and pushes outward.
        LipPuckerLeft, // Left part of the lip pinches inward and pushes outward.

        MouthUpperUpRight, // Upper right part of the lip is pushed upward.
        MouthUpperUpLeft, // Upper left part of the lip is pushed upward.
        MouthLowerDownRight, // Lower right part of the lip is pushed upward.
        MouthLowerDownLeft, // Lower left part of the lip is pushed upward.

        MouthUpperRight, // Moves upper lip right.
        MouthUpperLeft, // Moves upper lip left.
        MouthLowerRight, // Moves lower lip right.
        MouthLowerLeft, // Moves lower lip left.

        MouthSmileRight, // Right corner of the lip is pushed upward and outward.
        MouthSmileLeft, // Left corner of the lip is pushed upward and outward.
        MouthFrownRight, // Right corner of the lip is pushed downward.
        MouthFrownLeft, // Left corner of the lip is pushed downward.

        MouthDimpleRight, // Right corner of the lip is pushed backwards into the face, creating a dimple.
        MouthDimpleLeft, // Left corner of the lip is pushed backwards into the face, creating a dimple.
        MouthRaiserUpper, // Raises the upper part of the mouth in response to MouthRaiserLower.
        MouthRaiserLower, // Raises the lower part of the mouth
        MouthPressRight, // Squeezes the right side lips together vertically and flattens them.
        MouthPressLeft, // Squeezes the left side lips together vertically and flattens them.
        MouthTightenerRight, // Squeezes the right side lips together horizontally and thickens them vertically slightly.
        MouthTightenerLeft, // Squeezes the right side lips together horizontally and thickens them vertically slightly.
        MouthStretchRight, // Stretches the right side lips together horizontally and thins them vertically slightly.
        MouthStretchLeft,  // Stretches the left side lips together horizontally and thins them vertically slightly.

        #endregion

        #region Tongue Expressions

        TongueOut, // Combined LongStep1 and LongStep2 into one shape, as it can be emulated in-animation

        TongueUp, // Tongue points in an upward direction.
        TongueDown, // Tongue points in an downward direction.
        TongueRight, // Tongue points in an rightward direction.
        TongueLeft, // Tongue points in an leftward direction.

        // Based on https://www.naun.org/main/NAUN/computers/2018/a042007-060.pdf
        TongueRoll, // Rolls up the sides of the tongue into a 'hotdog bun' shape.
        TongueBend, // Pushes tip of the tongue below the rest of the tongue in an arch.
        TongueCurl, // Pushes tip of the tongue above the rest of the tongue in an arch.
        TongueSquish, // Tongue becomes thinner width-wise and slightly thicker height-wise.
        TongueFlat, // Tongue becomes thicker width-wise and slightly thinner height-wise.

        TongueTwistRight, // Tongue tip rotates clockwise (from POV) with the rest of the tongue following gradually.
        TongueTwistLeft, // Tongue tip rotates counter-clockwise (from POV) with the rest of the tongue following gradually.

        #endregion

        Max
    }

    /// <summary> 
    /// Represents the type of Legacy Shape data being sent by UnifiedExpressionData, in the form of enumerated SRanipal shapes.
    /// </summary>
    /// <remarks>
    /// This enum is not intended to be used directly by modules in the final iteration, and instead will be used as a compatibility layer for making the new Unified system backwards compatible with older VRCFT avatars.
    /// </remarks>
    internal enum SRanipal_LipShape_v2
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
