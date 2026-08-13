using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC.DataTypes;

namespace VRCFaceTracking.Core.Params.Expressions;

/// <summary>
/// Registers the optional face expression helper OSC parameters.
/// </summary>
public static class FaceExpressionOscParameters
{
    public static readonly Parameter[] Parameters =
    {
        new FaceExpressionOscParameterPair(),
    };
}

/// <summary>
/// Exposes the current helper expression index and power as OSC parameters.
/// </summary>
public sealed class FaceExpressionOscParameterPair : Parameter
{
    private readonly Parameter[] _parameters =
    {
        new BaseParam<int>("v2/FaceExpressionIndex", _ => GetIndex()),
        new BaseParam<float>("v2/FaceExpressionPower", _ => GetPower()),
    };

    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        return _parameters.SelectMany(parameter => parameter.ResetParam(newParams)).ToArray();
    }

    public override (string, Parameter)[] GetParamNames()
    {
        return _parameters.SelectMany(parameter => parameter.GetParamNames()).ToArray();
    }

    private static int GetIndex()
    {
        var snapshot = FaceExpressionOscRuntime.GetSnapshot();
        return snapshot.Enabled ? snapshot.CurrentResult.Index : 0;
    }

    private static float GetPower()
    {
        var snapshot = FaceExpressionOscRuntime.GetSnapshot();
        return snapshot.Enabled
            ? MathF.Round(Math.Clamp(snapshot.CurrentResult.Power, 0f, 1f), 3)
            : 0f;
    }
}
