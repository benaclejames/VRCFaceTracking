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
    public float[] smoothingData = new float[(int)UnifiedExpressions.Max];
    public float gazeSmoothness, pupilSmoothness, opennessSmoothness;
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
                    smoothingData[i]
                );
        }
        
        data.Eye.Left.Openness = SimpleLerp(data.Eye.Left.Openness, buffer.Eye.Left.Openness, opennessSmoothness);
        data.Eye.Left.PupilDiameter_MM = SimpleLerp(data.Eye.Left.PupilDiameter_MM, buffer.Eye.Left.PupilDiameter_MM, pupilSmoothness);
        data.Eye.Left.Gaze = SimpleLerp(data.Eye.Left.Gaze, buffer.Eye.Left.Gaze, pupilSmoothness);

        data.Eye.Right.Openness = SimpleLerp(data.Eye.Right.Openness, buffer.Eye.Right.Openness, pupilSmoothness);
        data.Eye.Right.PupilDiameter_MM = SimpleLerp(data.Eye.Right.PupilDiameter_MM, buffer.Eye.Right.PupilDiameter_MM, pupilSmoothness);
        data.Eye.Right.Gaze = SimpleLerp(data.Eye.Right.Gaze, buffer.Eye.Right.Gaze, pupilSmoothness);
    }

    public void Initialize() => Reset();

    public void Reset() 
    {
        for (int i = 0; i < smoothingData.Length ; i++)
            smoothingData[i] = SMOOTH_INIT;
    }
    public UnifiedMutationProperty[] GetProperties()
    {
        List<UnifiedMutationProperty> props = new List<UnifiedMutationProperty>
        {
            new UnifiedMutationProperty { Name = "SmoothArray", Value = smoothingData },
            new UnifiedMutationProperty { Name = "Gaze", Value = gazeSmoothness },
            new UnifiedMutationProperty { Name = "Pupil", Value = pupilSmoothness },
            new UnifiedMutationProperty { Name = "Openness", Value = opennessSmoothness }
        };
        return props.ToArray();
    }

    public void SetProperties(UnifiedMutationProperty[] props)
    {
        if (props != null &&
            props[0].Value.GetType() == typeof(float) &&
            (float)props[0].Value <= 1.0f &&
            (float)props[0].Value >= 0.0f)
        {
            foreach (UnifiedMutationProperty prop in props)
                switch (prop.Name)
                {
                    case "SmoothingArray":
                        smoothingData = (float[])prop.Value;
                        break;
                    case "Gaze":
                        gazeSmoothness = (float)prop.Value;
                        break;
                    case "Pupil":
                        pupilSmoothness = (float)prop.Value;
                        break;
                    case "Openness":
                        opennessSmoothness = (float)prop.Value;
                        break;
                    default: break;
                }
        }
    }
}