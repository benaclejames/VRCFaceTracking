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

        //Expanded Face Tracking Parameters
        BrowsInnerUp,
        BrowInnerUpLeft,
        BrowInnerUpRight,
        BrowsOuterUp,
        BrowOuterUpLeft,
        BrowOuterUpRight,
        BrowsDown,
        BrowDownLeft,
        BrowDownRight,
        EyesSquint,
        EyeSquintLeft,
        EyeSquintRight,
        CheekSquintLeft,
        CheekSquintRight,
        MouthRaiserUpper,
        MouthRaiserLower,
        MouthDimpleLeft,
        MouthDimpleRight,
        LipFunnelBottom,
        LipFunnelBottomLeft,
        LipFunnelBottomRight,
        LipFunnelTop,
        LipFunnelTopRight,
        LipFunnelTopLeft,
        MouthPress,
        MouthPressLeft,
        MouthPressRight,
        LipPuckerLeft, // Mouth Pout but not combined
        LipPuckerRight, // Mouth Pout but not combined
        MouthStretchLeft,
        MouthStretchRight,
        LipSuckTopLeft, // Cheek Suck but not combined
        LipSuckTopRight, // Cheek Suck but not combined
        LipSuckBottomLeft, // Cheek Suck but not combined
        LipSuckBottomRight, // Cheek Suck but not combined        
        MouthTightener,
        MouthTightenerLeft,
        MouthTightenerRight,
        //MouthClosed,
        NoseSneerLeft,
        NoseSneerRight,
        //JawDrop, // TESTING MOUTH APE SHAPE CONTROL
        Max
    }
}