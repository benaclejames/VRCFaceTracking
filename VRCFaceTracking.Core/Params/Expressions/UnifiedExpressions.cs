namespace VRCFaceTracking.Core.Params.Expressions
{
    // TODO: Create expressions map to relate legacy shapes to new ones

    /// <summary> 
    /// Represents the type of Shape data being sent by UnifiedExpressionData, in the form of enumerated shapes.
    /// </summary>
    /// <remarks>These shapes have a strong basis on the underlying muscular foundations of the entire head including the face, eyes, tongue, and inner mouth expressions.</remarks>
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
        //EyeClosedRight, // Closes the right eyelid. Basis on the overall constriction of the palpebral part of orbicularis oculi.
        //EyeClosedLeft, // Closes the left eyelid. Basis on the overall constriction of the palpebral part of orbicularis oculi.
        //EyeDilationRight, // Dilates the right eye's pupil
        //EyeDilationLeft, // Dilates the left eye's pupil
        //EyeConstrictRight, // Constricts the right eye's pupil
        //EyeConstrictLeft, // Constricts the left eye's pupil

        EyeSquintRight, // Squeezes the right eye socket muscles, causing the lower eyelid to constrict a little bit as well. Basis on the mostly lower constriction of the inner parts of the orbicularis oculi and the stressing of the muscle group as the eyelid is closed.
        EyeSquintLeft, // Squeezes the left eye socket muscles, causing the lower eyelid to constrict a little bit as well. Basis on the mostly lower constriction of the inner parts of the orbicularis oculi and the stressing of the muscle group as the eyelid is closed.
        EyeWideRight, // Right eyelid widens beyond the eyelid's relaxed position. Basis on the action of the levator palpebrae superioris.
        EyeWideLeft, // Left eyelid widens beyond the eyelid's relaxed position. Basis on the action of the levator palpebrae superioris.

        #endregion

        #region Eyebrow Expressions

        BrowPinchRight, // Inner right eyebrow pulls diagnally inwards and downwards slightly. Basis on the constriction of the corrugator supercilii muscle.
        BrowPinchLeft, // Inner left eyebrow pulls diagnally inwards and downwards slightly. Basis on the constriction of the corrugator supercilii muscle.
        BrowLowererRight, // Outer right eyebrow pulls downward. Basis on depressor supercilii, procerus, and partially the upper orbicularis oculi muscles action of lowering the eyebrow.
        BrowLowererLeft, // Outer Left eyebrow pulls downward. Basis on depressor supercilii, procerus, and partially the upper orbicularis oculi muscles action of lowering the eyebrow.
        BrowInnerUpRight, // Inner right eyebrow pulls upward. Basis on the inner grouping action of the frontal belly of the occipitofrontalis.
        BrowInnerUpLeft, // Inner left eyebrow pulls upward. Basis on the inner grouping action of the frontal belly of the occipitofrontalis.
        BrowOuterUpRight, // Outer right eyebrow pulls upward. Basis on the outer grouping action of the frontal belly of the occipitofrontalis.
        BrowOuterUpLeft, // Outer left eyebrow pulls upward. Basis on the outer grouping action of the frontal belly of the occipitofrontalis.

        #endregion

        #region Nose Expressions

        NasalDilationRight, // Right side nose's canal dilates. Basis on the alar nasalis muscle.
        NasalDilationLeft, // Left side nose's canal dilates. Basis on the alar nasalis muscle.
        NasalConstrictRight, // Right side nose's canal constricts. Basis on the transverse nasalis muscle.
        NasalConstrictLeft, // Left side nose's canal constricts. Basis on the transverse nasalis muscle.

        #endregion

        #region Cheek Expressions

        CheekSquintRight, // Raises the right side cheek. Basis on the main action of the lower outer part of the orbicularis oculi.
        CheekSquintLeft, // Raises the left side cheek. Basis on the main action of the lower outer part of the orbicularis oculi.
        CheekPuffRight, // Puffs the right side cheek. Basis on the cheeks' ability to stretch orbitally.
        CheekPuffLeft, // Puffs the left side cheek. Basis on the cheeks' ability to stretch orbitally.
        CheekSuckRight, // Sucks in the right side cheek. Basis on the cheeks' ability to stretch inwards from suction.
        CheekSuckLeft, // Sucks in the left side cheek. Basis on the cheeks' ability to stretch inwards from suction.

        #endregion

        #region Jaw Exclusive Expressions

        JawOpen, // Opens the jawbone. Basis of the general action of the jaw opening by the masseter and temporalis muscle grouping.
        JawRight, // Pushes the jawbone right. Basis on medial pterygoid and lateral pterygoid's general action of shifting the jaw sideways.
        JawLeft, // Pushes the jawbone left. Basis on medial pterygoid and lateral pterygoid's general action of shifting the jaw sideways.
        JawForward, // Pushes the jawbone forward. Basis on the lateral pterygoid's ability to shift the jaw forward.
        JawBackward, // Pulls the jawbone backwards slightly. Based on the retraction of the temporalis muscle.
        JawClench, // Specific jaw muscles that forces the jaw closed. Causes the masseter muscle (visible close to the back of the jawline) to visibly flex.
        JawMandibleRaise, // Raises mandible (jawbone).

        MouthClosed, // Closes the mouth relative to JawOpen. Basis on the complex tightening action of the orbicularis oris muscle.

        #endregion

        #region Lip Expressions

        // 'Lip Push/Pull' group
        LipSuckUpperRight, // Upper right part of the lip gets tucked inside the mouth. No direct muscle basis as this action is caused from many indirect movements of tucking the lips.
        LipSuckUpperLeft, // Upper left part of the lip gets tucked inside the mouth. No direct muscle basis as this action is caused from many indirect movements of tucking the lips.
        LipSuckLowerRight, // Lower right part of the lip gets tucked inside the mouth. No direct muscle basis as this action is caused from many indirect movements of tucking the lips.
        LipSuckLowerLeft, // Lower left part of the lip gets tucked inside the mouth. No direct muscle basis as this action is caused from many indirect movements of tucking the lips.

        LipSuckCornerRight, // The right corners of the lips fold inward and into the mouth. Basis on the lips ability to stretch inwards from suction.
        LipSuckCornerLeft, // The left corners of the lips fold inward and into the mouth. Basis on the lips ability to stretch inwards from suction.

        LipFunnelUpperRight, // Upper right part of the lip pushes outward into a funnel shape. Basis on the orbicularis oris orbital muscle around the mouth pushing outwards and puckering.
        LipFunnelUpperLeft, // Upper left part of the lip pushes outward into a funnel shape. Basis on the orbicularis oris orbital muscle around the mouth pushing outwards and puckering.
        LipFunnelLowerRight, // Lower right part of the lip pushes outward into a funnel shape. Basis on the orbicularis oris orbital muscle around the mouth pushing outwards and puckering.
        LipFunnelLowerLeft, // Lower left part of the lip pushes outward into a funnel shape. Basis on the orbicularis oris orbital muscle around the mouth pushing outwards and puckering.

        LipPuckerUpperRight, // Upper right part of the lip pinches inward and pushes outward. Basis on complex action of the orbicularis-oris orbital muscle around the lips.
        LipPuckerUpperLeft, // Upper left part of the lip pinches inward and pushes outward. Basis on complex action of the orbicularis-oris orbital muscle around the lips.
        LipPuckerLowerRight, // Lower right part of the lip pinches inward and pushes outward. Basis on complex action of the orbicularis-oris orbital muscle around the lips.
        LipPuckerLowerLeft, // Lower left part of the lip pinches inward and pushes outward. Basis on complex action of the orbicularis-oris orbital muscle around the lips.

        // 'Upper lip raiser' group
        MouthUpperUpRight, // Upper right part of the lip is pulled upward. Basis on the levator labii superioris muscle.
        MouthUpperUpLeft, // Upper left part of the lip is pulled upward. Basis on the levator labii superioris muscle.
        MouthUpperDeepenRight, // Upper outer right part of the lip is pulled upward, backward, and rightward. Basis on the zygomaticus minor muscle.
        MouthUpperDeepenLeft, // Upper outer left part of the lip is pulled upward, backward, and rightward. Basis on the zygomaticus minor muscle.
        NoseSneerRight, // The right side face pulls upward into a sneer and raises the inner part of the lips at extreme ranges. Based on levator labii superioris alaeque nasi muscle.
        NoseSneerLeft, // The right side face pulls upward into a sneer and raises the inner part of the lips slightly at extreme ranges. Based on levator labii superioris alaeque nasi muscle.

        // 'Lower lip depressor' group
        MouthLowerDownRight, // Lower right part of the lip is pulled downward. Basis on the depressor labii inferioris muscle.
        MouthLowerDownLeft, // Lower left part of the lip is pulled downward. Basis on the depressor labii inferioris muscle.

        // 'Mouth Direction' group
        MouthUpperRight, // Moves upper lip right. Basis on the general horizontal movement action of the upper orbicularis oris orbital, levator anguli oris, and buccinator muscle grouping.
        MouthUpperLeft, // Moves upper lip left. Basis on the general horizontal movement action of the upper orbicularis oris orbital, levator anguli oris, and buccinator muscle grouping.
        MouthLowerRight, // Moves lower lip right. Basis on the general horizontal movement action of the lower orbicularis oris orbital, risorius, depressor labii inferioris, and buccinator muscle grouping.
        MouthLowerLeft, // Moves lower lip left. Basis on the general horizontal movement action of the lower orbicularis oris orbital, risorius, depressor labii inferioris, and buccinator muscle grouping.

        // 'Smile' group
        MouthCornerPullRight, // Right side of the lip is pulled diagnally upwards and rightwards significantly. Basis on the action of the levator anguli oris muscle.
        MouthCornerPullLeft, // :eft side of the lip is pulled diagnally upwards and leftwards significantly. Basis on the action of the levator anguli oris muscle.
        MouthCornerSlantRight, // Right corner of the lip is pulled upward slightly. Basis on the action of the levator anguli oris muscle.
        MouthCornerSlantLeft, // Left corner of the lip is pulled upward slightly. Basis on the action of the levator anguli oris muscle.

        // 'Sad' group
        MouthFrownRight, // Right corner of the lip is pushed downward. Basis on the action of the depressor anguli oris muscle. Directly opposes the levator muscles.
        MouthFrownLeft, // Left corner of the lip is pushed downward. Basis on the action of the depressor anguli oris muscle. Directly opposes the levator muscles.
        MouthStretchRight, // Stretches the right side lips together horizontally and thins them vertically slightly. Basis on the risorius muscle.
        MouthStretchLeft,  // Stretches the left side lips together horizontally and thins them vertically slightly. Basis on the risorius muscle.

        MouthDimpleRight, // Right corner of the lip is pushed backwards into the face, creating a dimple. Basis on buccinator muscle structure.
        MouthDimpleLeft, // Left corner of the lip is pushed backwards into the face, creating a dimple. Basis on buccinator muscle structure.

        MouthRaiserUpper, // Raises the upper part of the mouth in response to MouthRaiserLower. No muscular basis.
        MouthRaiserLower, // Raises the lower part of the mouth. Based on the complex lower pushing action of the mentalis muscle.
        MouthPressRight, // Squeezes the right side lips together vertically and flattens them. Basis on the complex tightening action of the orbicularis oris muscle.
        MouthPressLeft, // Squeezes the left side lips together vertically and flattens them. Basis on the complex tightening action of the orbicularis oris muscle.
        MouthTightenerRight, // Squeezes the right side lips together horizontally and thickens them vertically slightly. Basis on the complex tightening action of the orbicularis oris muscle.
        MouthTightenerLeft, // Squeezes the right side lips together horizontally and thickens them vertically slightly. Basis on the complex tightening action of the orbicularis oris muscle.

        #endregion

        #region Tongue Expressions

        TongueOut, // Combined LongStep1 and LongStep2 into one shape, as it can be emulated in-animation

        // Based on SRanipal tracking standard's tongue tracking.
        TongueUp, // Tongue points in an upward direction.
        TongueDown, // Tongue points in an downward direction.
        TongueRight, // Tongue points in an rightward direction.
        TongueLeft, // Tongue points in an leftward direction.

        // Based on https://www.naun.org/main/NAUN/computers/2018/a042007-060.pdf
        TongueRoll, // Rolls up the sides of the tongue into a 'hotdog bun' shape.
        TongueBendDown, // Pushes tip of the tongue below the rest of the tongue in an arch.
        TongueCurlUp, // Pushes tip of the tongue above the rest of the tongue in an arch.
        TongueSquish, // Tongue becomes thinner width-wise and slightly thicker height-wise.
        TongueFlat, // Tongue becomes thicker width-wise and slightly thinner height-wise.

        TongueTwistRight, // Tongue tip rotates clockwise from POV with the rest of the tongue following gradually.
        TongueTwistLeft, // Tongue tip rotates counter-clockwise from POV with the rest of the tongue following gradually.

        #endregion

        #region Throat/Neck Expressions

        SoftPalateClose, // Visibly lowers the back of the throat (soft palate) inside the mouth to close off the throat.
        ThroatSwallow, // Visibly causes the Adam's apple to pull upward into the throat as if swallowing.
        
        NeckFlexRight, // Flexes the Right side of the neck and face (causes the right corner of the face to stretch towards.)
        NeckFlexLeft, // Flexes the left side of the neck and face (causes the left corner of the face to stretch towards.)

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
