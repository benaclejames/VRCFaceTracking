using VRCFaceTracking.Core.Params.DataTypes;

namespace VRCFaceTracking.Core.Params.Expressions;
public static class UnifiedHeadParameters
{
    public static readonly Parameter[] HeadParameters = {
        #region Rotation

        new EParam("v2/Head/Yaw", exp => exp.Head.HeadYaw),
        new EParam("v2/Head/Pitch", exp => exp.Head.HeadPitch),
        new EParam("v2/Head/Roll", exp => exp.Head.HeadRoll),

        #endregion 

        #region Position

        new EParam("v2/Head/PosX", exp => exp.Head.HeadPosX),
        new EParam("v2/Head/PosY", exp => exp.Head.HeadPosY),
        new EParam("v2/Head/PosZ", exp => exp.Head.HeadPosZ)

        #endregion
    };
}
