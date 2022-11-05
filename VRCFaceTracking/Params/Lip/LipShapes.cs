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

        CheekRaiserLeft,
        CheekRaiserRight,
        ChinRaiserBottom,
        ChinRaiserTop,
        DimplerLeft,
        DimplerRight,
        LipFunnelerLeftBottom,
        LipFunnelerLeftTop,
        LipFunnelerRightBottom,
        LipFunnelerRightTop,
        LipPressorLeft,
        LipPressorRight,
        LipPuckerLeft, // Mouth Pout but not combined
        LipPuckerRight, // Mouth Pout but not combined
        LipStretcherLeft,
        LipStretcherRight,
        LipSuckLeftBottom, // Cheek Suck but not combined
        LipSuckLeftTop, // Cheek Suck but not combined
        LipSuckRightBottom, // Cheek Suck but not combined
        LipSuckRightTop, // Cheek Suck but not combined
        LipTightenerLeft,
        LipTightenerRight,
        LowerLipDepressorLeft,
        LowerLipDepressorRight,
        NoseWrinklerLeft,
        NoseWrinklerRight,
        Max
    }
}