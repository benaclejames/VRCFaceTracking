namespace VRCFaceTracking.Params.Lip
{
    // TODO: Create expressions map to relate legacy shapes to new ones
    public enum UnifiedExpression
    {
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

        CheekRaiserL,
        CheekRaiserR,
        ChinRaiserB,
        ChinRaiserT,
        DimplerL,
        DimplerR,
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
        Max
    }
}