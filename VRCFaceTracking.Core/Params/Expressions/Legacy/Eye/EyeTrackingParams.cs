using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.DataTypes;

namespace VRCFaceTracking.Core.Params.Expressions.Legacy.Eye
{
    public static class EyeTrackingParams
    {
        public static readonly Parameter[] ParameterList = {
            #region XYParams
            
            new EParam("Eyes", exp => exp.Eye.Combined().Gaze),
            new EParam("LeftEye", exp => exp.Eye.Left.Gaze),
            new EParam("RightEye", exp => exp.Eye.Right.Gaze),
            
            #endregion
            
            #region Widen

            new EParam("LeftEyeWiden", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight),
            new EParam("RightEyeWiden", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight),
            new EParam("EyeWiden", exp => (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f),
            
            #endregion
            
            #region Squeeze
            
            new EParam("LeftEyeSqueeze", exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight),
            new EParam("RightEyeSqueeze", exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight),
            new EParam("EyesSqueeze", exp => (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f),
            
            #endregion
            
            #region Dilation
            
            new EParam("EyesDilation", exp => exp.Eye.Combined().PupilDiameter_MM),
            new EParam("EyesPupilDiameter", exp => (exp.Eye.Left.PupilDiameter_MM + exp.Eye.Right.PupilDiameter_MM) * .5f),
            
            #endregion
            
            #region EyeLid
            
            new EParam("LeftEyeLid", exp => exp.Eye.Left.Openness),
            new EParam("RightEyeLid", exp => exp.Eye.Right.Openness),
            new EParam("CombinedEyeLid", exp => (exp.Eye.Left.Openness + exp.Eye.Right.Openness) / 2.0f),
            
            #endregion
            
            #region EyeLidExpanded
            
            new EParam("LeftEyeLidExpanded", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * 0.2f + exp.Eye.Left.Openness * 0.8f, 0.5f, true),
            new EParam("RightEyeLidExpanded", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * 0.2f + exp.Eye.Right.Openness * 0.8f, 0.5f, true),
            new EParam("EyeLidExpanded", exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight +
                 exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) * .1f +
                (exp.Eye.Left.Openness + exp.Eye.Right.Openness) * .4f, 0.5f, true),

            #endregion

            #region EyeLidExpandedSqueeze

            new EParam("LeftEyeLidExpandedSqueeze", exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f -
                Squeeze(exp, 0), 0.5f, true),
            new EParam("RightEyeLidExpandedSqueeze", exp =>
                exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f -
                Squeeze(exp, 1), 0.5f, true),
            new EParam("EyeLidExpandedSqueeze", exp =>
                ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight +
                  exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) * .2f +
                 (exp.Eye.Left.Openness + exp.Eye.Right.Openness) * .8f -
                 Squeeze(exp, 0) - Squeeze(exp, 1)) * .5f, 0.5f, true),

            #endregion
            
            #region EyeLidExpanded Binary
            
            new BinaryBaseParameter("LeftEyeLidExpanded", exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f > .8f)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight;
                return exp.Eye.Left.Openness;
            }),

            new BinaryBaseParameter("RightEyeLidExpanded", exp =>
            {
                if (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f > .8f)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight;
                return exp.Eye.Right.Openness;
            }),

            new BinaryBaseParameter("CombinedEyeLidExpanded", exp =>
            {
                if ((exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f > 0)
                    return (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) / 2.0f;
                return exp.Eye.Combined().Openness;
            }),

            #endregion
            
            #region EyeLidExpandedSqueeze Binary
            
            // Still need to reforumulate these parameters with the compensation.
            new BinaryBaseParameter("LeftEyeLidExpandedSqueeze", exp =>
            {
                var eyeLid = exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f - Squeeze(exp, 0);
                if (eyeLid > .8f)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight;
                if (eyeLid >= 0)
                    return exp.Eye.Left.Openness;
                return Squeeze(exp, 0);
            }),
            
            new BinaryBaseParameter("RightEyeLidExpandedSqueeze", exp =>
            {
                var eyeLid = exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f - Squeeze(exp, 1);
                if (eyeLid > .8f)
                    return exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight; 
                if (eyeLid >= 0)
                    return exp.Eye.Right.Openness;
                return Squeeze(exp, 1);
            }),
            
            new BinaryBaseParameter("CombinedEyeLidExpandedSqueeze", exp =>
            {
                var eyeLid = (
                    (exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight +
                     exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight) * .2f +
                    (exp.Eye.Left.Openness + exp.Eye.Right.Openness) * .8f -
                    Squeeze(exp, 0) - Squeeze(exp, 1)) * .5f;
                
                return eyeLid switch
                {
                    > .8f => (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight +
                              exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight) * .5f,
                    >= 0 => exp.Eye.Combined().Openness,
                    _ => (Squeeze(exp, 0) + Squeeze(exp, 1)) * .5f
                };
            }),
            
            #endregion

            #region EyeLidExpandedSupplemental

            // These parameters are used to distinguish when EyeLidExpanded / EyeLidExpandedSqueeze
            // is returning a value as a Widen or Squeeze. Intended for the Bool or Binary param variant.
            new BaseParam<bool>("LeftEyeWidenToggle", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f > .8f),
            new BaseParam<bool>("RightEyeWidenToggle", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f > .8f),
            new BaseParam<bool>("EyesWidenToggle", exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f + 
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f) / 2.0f
                > .8f),

            new BaseParam<bool>("LeftEyeSqueezeToggle", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f - Squeeze(exp, 0) < 0),
            new BaseParam<bool>("RightEyeSqueezeToggle", exp => exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f - Squeeze(exp, 1) < 0),
            new BaseParam<bool>("EyesSqueezeToggle", exp =>
                (exp.Shapes[(int)UnifiedExpressions.EyeWideRight].Weight * .2f + exp.Eye.Right.Openness * .8f - Squeeze(exp, 1) +
                exp.Shapes[(int)UnifiedExpressions.EyeWideLeft].Weight * .2f + exp.Eye.Left.Openness * .8f - Squeeze(exp, 0)) / 2.0f
                < 0),

            #endregion

            // These parameters were introduced to retroactively support Quest Pro's extra tracking.

            #region Quest Pro Legacy

            new EParam("BrowsInnerUp", exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight 
                                            > exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight 
                ? exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight 
                : exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight),

            new EParam("BrowInnerUpLeft", exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft].Weight),
            new EParam("BrowInnerUpRight", exp => exp.Shapes[(int)UnifiedExpressions.BrowInnerUpRight].Weight),

            new EParam("BrowsOuterUp", exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight 
                                            > exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight 
                ? exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight
                : exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight),

            new EParam("BrowOuterUpLeft", exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft].Weight),
            new EParam("BrowOuterUpRight", exp => exp.Shapes[(int)UnifiedExpressions.BrowOuterUpRight].Weight),

            new EParam("BrowsDown", exp => (exp.Shapes[(int)UnifiedExpressions.BrowPinchLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight) / 2.0f
                                         > (exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight) / 2.0f
                ? (exp.Shapes[(int)UnifiedExpressions.BrowPinchLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight) / 2.0f
                : (exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight) / 2.0f),

            new EParam("BrowDownLeft", exp => (exp.Shapes[(int)UnifiedExpressions.BrowPinchLeft].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererLeft].Weight) / 2.0f),
            new EParam("BrowDownRight", exp => (exp.Shapes[(int)UnifiedExpressions.BrowPinchRight].Weight + exp.Shapes[(int)UnifiedExpressions.BrowLowererRight].Weight) / 2.0f),

            new EParam("MouthRaiserLower", exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserLower].Weight),
            new EParam("MouthRaiserUpper", exp => exp.Shapes[(int)UnifiedExpressions.MouthRaiserUpper].Weight),

            new EParam("EyesSquint", exp => (exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight) / 2.0f),
            new EParam("EyeSquintLeft", exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight),
            new EParam("EyeSquintRight", exp => exp.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight),

            new EParam("CheeksSquint", exp => (exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight + exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight) / 2.0f),
            new EParam("CheekSquintLeft", exp => exp.Shapes[(int)UnifiedExpressions.CheekSquintLeft].Weight),
            new EParam("CheekSquintRight", exp => exp.Shapes[(int)UnifiedExpressions.CheekSquintRight].Weight),

            new EParam("MouthDimple", exp => (exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight) / 2.0f),
            new EParam("MouthDimpleLeft", exp => exp.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].Weight),
            new EParam("MouthDimpleRight", exp => exp.Shapes[(int)UnifiedExpressions.MouthDimpleRight].Weight),

            new EParam("MouthPress", exp => (exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight) / 2.0f),
            new EParam("MouthPressLeft", exp => exp.Shapes[(int)UnifiedExpressions.MouthPressLeft].Weight),
            new EParam("MouthPressRight", exp => exp.Shapes[(int)UnifiedExpressions.MouthPressRight].Weight),

            new EParam("MouthStretch", exp => (exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight) / 2.0f),
            new EParam("MouthStretchLeft", exp => exp.Shapes[(int)UnifiedExpressions.MouthStretchLeft].Weight),
            new EParam("MouthStretchRight", exp => exp.Shapes[(int)UnifiedExpressions.MouthStretchRight].Weight),

            new EParam("MouthTightener", exp => (exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight + exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight) / 2.0f),
            new EParam("MouthTightenerLeft", exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerLeft].Weight),
            new EParam("MouthTightenerRight", exp => exp.Shapes[(int)UnifiedExpressions.MouthTightenerRight].Weight),

            new EParam("NoseSneer", exp => (exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight + exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight) / 2.0f),
            new EParam("NoseSneerLeft", exp => exp.Shapes[(int)UnifiedExpressions.NoseSneerLeft].Weight),
            new EParam("NoseSneerRight", exp => exp.Shapes[(int)UnifiedExpressions.NoseSneerRight].Weight),

            #endregion
        };

        // eyeIndex: 0 == Left, 1 == Right
        private static float Squeeze(UnifiedTrackingData data, int eyeIndex)
        {
            if (eyeIndex == 0)
            {
                return (float)(1.0f - Math.Pow(data.Eye.Left.Openness, .15)) *
                       data.Shapes[(int)UnifiedExpressions.EyeSquintLeft].Weight;
            }

            return (float)(1.0f - Math.Pow(data.Eye.Right.Openness, .15)) *
                   data.Shapes[(int)UnifiedExpressions.EyeSquintRight].Weight;
        }
    }
}