using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Mutators;

public class SmoothingMutator : IUnifiedMutation
{
    public string Name => "Smoothing";
    public bool Mutable { get; set; }
    public int Order => 100;
    public class SmoothingData
    {
        public float[] expressions = new float[(int)UnifiedExpressions.Max];
        public float gazeSmoothness, pupilSmoothness, opennessSmoothness;
    }
    public SmoothingData smoothingData = new();
    private const float SMOOTH_INIT = .9f;

    static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

    public void Mutate(ref UnifiedTrackingData data, UnifiedTrackingData buffer, ILogger<UnifiedTrackingMutator> _logger)
    {
        for (int i = 0; i < data.Shapes.Length; i++)
        {
            data.Shapes[i].Weight =
                SimpleLerp(
                    data.Shapes[i].Weight,
                    buffer.Shapes[i].Weight,
                    smoothingData.expressions[i]
                );
        }
        
        data.Eye.Left.Openness = SimpleLerp(data.Eye.Left.Openness, buffer.Eye.Left.Openness, smoothingData.opennessSmoothness);
        data.Eye.Left.PupilDiameter_MM = SimpleLerp(data.Eye.Left.PupilDiameter_MM, buffer.Eye.Left.PupilDiameter_MM, smoothingData.pupilSmoothness);
        data.Eye.Left.Gaze = SimpleLerp(data.Eye.Left.Gaze, buffer.Eye.Left.Gaze, smoothingData.pupilSmoothness);

        data.Eye.Right.Openness = SimpleLerp(data.Eye.Right.Openness, buffer.Eye.Right.Openness, smoothingData.pupilSmoothness);
        data.Eye.Right.PupilDiameter_MM = SimpleLerp(data.Eye.Right.PupilDiameter_MM, buffer.Eye.Right.PupilDiameter_MM, smoothingData.pupilSmoothness);
        data.Eye.Right.Gaze = SimpleLerp(data.Eye.Right.Gaze, buffer.Eye.Right.Gaze, smoothingData.pupilSmoothness);
    }

    public void Initialize() => Reset();

    public void Reset() 
    {
        for (int i = 0; i < smoothingData.expressions.Length ; i++)
            smoothingData.expressions[i] = SMOOTH_INIT;
    }
    public object GetProperties() => smoothingData;

    public void SetProperties(object data) =>
        smoothingData = data as SmoothingData;
}