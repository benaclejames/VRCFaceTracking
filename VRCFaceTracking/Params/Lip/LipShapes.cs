namespace VRCFaceTracking.Params.Lip
{
    // TODO Remove duplicate expressions post testing!
    public enum VRCFTLipShape
    {
        #region SRanipal
        JawRight, // +JawX
        JawLeft, // -JawX
        JawForward,
        JawOpen,
        MouthApeShape,
        MouthUpperRight, // +MouthUpper
        MouthUpperLeft, // -MouthUpper
        MouthLowerRight, // +MouthLower
        MouthLowerLeft, // -MouthLower
        MouthUpperOverturn,
        MouthLowerOverturn,
        MouthPout,
        MouthSmileRight, // +SmileSadRight
        MouthSmileLeft, // +SmileSadLeft
        MouthSadRight, // -SmileSadRight
        MouthSadLeft, // -SmileSadLeft
        CheekPuffRight,
        CheekPuffLeft,
        CheekSuck,
        MouthUpperUpRight,
        MouthUpperUpLeft,
        MouthLowerDownRight,
        MouthLowerDownLeft,
        MouthUpperInside,
        MouthLowerInside,
        MouthLowerOverlay,
        TongueLongStep1,
        TongueLongStep2,
        TongueDown, // -TongueY
        TongueUp, // +TongueY
        TongueRight, // +TongueX
        TongueLeft, // -TongueX
        TongueRoll,
        TongueUpLeftMorph,
        TongueUpRightMorph,
        TongueDownLeftMorph,
        TongueDownRightMorph,
        SRanipalMax,
        #endregion

        #region Custom Shapes
        BrowLowererL,
        BrowLowererR,
        CheekRaiserL,
        CheekRaiserR,
        ChinRaiserB,
        ChinRaiserT,
        DimplerL,
        DimplerR,
        InnerBrowRaiserL,
        InnerBrowRaiserR,
        LipFunnelerLB,
        LipFunnelerLT,
        LipFunnelerRB,
        LipFunnelerRT,
        LipPressorL,
        LipPressorR,
        LipPuckerL, // Mouth Pout but not combined
        LipPuckerR, // Mouth Pout but not combined
        LipStretcherL,
        LipStretcherR,
        LipSuckLB, // Cheek Suck but not combined
        LipSuckLT, // Cheek Suck but not combined
        LipSuckRB, // Cheek Suck but not combined
        LipSuckRT, // Cheek Suck but not combined
        LipTightenerL,
        LipTightenerR,
        LowerLipDepressorL,
        LowerLipDepressorR,
        NoseWrinklerL,
        NoseWrinklerR,
        OuterBrowRaiserL,
        OuterBrowRaiserR,
        Max
        #endregion

        #region Remarks
        /* 
        * TODO: Add ARKit shapes
        * ...
        * 
        * FACS Shapes not tracked:
        * CheekPuffL
        * CheekPuffR
        * CheekSuckL
        * CheekSuckR
        * EyesClosedL // Eye Shapes not tracked in Lip Shapes
        * EyesClosedR // Eye Shapes not tracked in Lip Shapes
        * EyesLookDownL // Eye Shapes not tracked in Lip Shapes
        * EyesLookDownR // Eye Shapes not tracked in Lip Shapes
        * EyesLookLeftL // Eye Shapes not tracked in Lip Shapes
        * EyesLookLeftR // Eye Shapes not tracked in Lip Shapes
        * EyesLookRightL // Eye Shapes not tracked in Lip Shapes
        * EyesLookRightR // Eye Shapes not tracked in Lip Shapes
        * EyesLookUpL // Eye Shapes not tracked in Lip Shapes
        * EyesLookUpR // Eye Shapes not tracked in Lip Shapes
        * JawDrop // Jaw Open
        * JawSidewaysLeft // Jaw Left
        * JawSidewaysRight // Jaw Right
        * JawThrust // Jaw Forward
        * LidTightenerL Added to eye shape!
        * LidTightenerR Added to eye shape!
        * LipCornerDepressorL // Mouth Sad Left
        * LipCornerDepressorR // Mouth Sad Right
        * LipCornerPullerL // Mouth Smile Left
        * LipCornerPullerR // Mouth Smile Right
        * LipsToward // MouthUpperOverturn
        * MouthLeft
        * MouthRight
        * UpperLidRaiserL // MouthUpperUpLeft
        * UpperLidRaiserR // MouthUpperUpRight
        * UpperLipRaiserL // MouthUpperDownLeft
        * UpperLipRaiserR // MouthUpperDownRight
        */
        #endregion Remarks
    }
}