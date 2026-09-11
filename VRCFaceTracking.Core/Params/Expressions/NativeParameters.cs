using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Params.DataTypes;
using VRCFaceTracking.Core.Params.Expressions.Legacy.Eye;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Params.Expressions;

public class VRCNativeParameters
{
    private static (string paramName, Parameter paramLiteral)[] IsEyeParameter(IParameterDefinition[] newParams)
    {
        // Get all the names of all parameters in both the unified tracking list and the old legacy eye list
        var allParams = UnifiedTracking.AllParameters_v2.Concat(EyeTrackingParams.ParameterList).SelectMany(p => p.GetParamNames());
                
        // Now we match parameters to the literals as a sort of sanity check (we use endswith since we don't know prefix. theres prob cases where binary can slip through the cracks)
        return allParams.Where(p => newParams.Any(pd => pd.Address.EndsWith(p.paramName))).ToArray();
    }

    public static readonly Parameter[] NativeParameters =
    {
        // Use when tracking interface is sending verbose gaze data.
        /*new NativeParameter<Vector2>(exp =>
            new Vector2(exp.Eye.Combined().Gaze.ToPitch(),
                        exp.Eye.Combined().Gaze.ToYaw()),
            param =>
                IsEyeParameter(
                    param.Where(p =>
                    p.Name.Contains("Eye") &&
                    (p.Name.Contains("Left") || p.Name.Contains("Right") || p.Name.Contains("Eyes")) &&
                    (p.Name.Contains('X') || p.Name.Contains('Y'))).ToArray()
                )
                .Length == 0,
            "/tracking/eye/CenterPitchYaw"
            ),*/

        // Use when tracking interface is sending combined gaze data.
        new NativeParameter<Vector4>(exp =>
                new Vector4(exp.Eye.Left.Gaze.ToPitch(),
                    exp.Eye.Left.Gaze.ToYaw(),
                    exp.Eye.Right.Gaze.ToPitch(),
                    exp.Eye.Right.Gaze.ToYaw()),
            param =>
                IsEyeParameter(
                        param.Where(p =>
                            p.Name.Contains("Eye") &&
                            (p.Name.Contains('X') || p.Name.Contains('Y'))).ToArray())
                    .Length == 0,
            "/tracking/eye/LeftRightPitchYaw"
        ),


        new NativeParameter<float>(
            exp => 1 - exp.Eye.Combined().Openness,
            param => IsEyeParameter(
                    param.Where(p =>
                        p.Name.Contains("Eye") &&
                        (p.Name.Contains("Open") || p.Name.Contains("Lid"))).ToArray())
                .Length == 0,
            "/tracking/eye/EyesClosedAmount"
        )
    };
}