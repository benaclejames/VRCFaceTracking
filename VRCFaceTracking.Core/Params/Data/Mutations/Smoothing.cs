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
    private const float SMOOTH_INIT = .9f;

    static float SimpleLerp(float input, float previousInput, float value) => input * (1.0f - value) + previousInput * value;

    public void Mutate(ref UnifiedTrackingData data, UnifiedTrackingData buffer, ILogger<UnifiedTrackingMutator> _logger)
    {
        for (int i = 0; i < data.Shapes.Length; i++)
        {
            //data.Shapes[i].Weight = 1f;
            //buffer.Shapes[i].Weight = 1f;
            //data.Shapes[i].Weight = smoothingData[i];

            data.Shapes[i].Weight =
                SimpleLerp(
                    data.Shapes[i].Weight,
                    buffer.Shapes[i].Weight,
                    smoothingData[i]
                );
        }
        
        /*
        data.Eye.Left.Openness = SimpleLerp(data.Eye.Left.Openness, buffer.Eye.Left.Openness, smoothingData[]);
        input.Eye.Left.PupilDiameter_MM = SimpleLerp(input.Eye.Left.PupilDiameter_MM, trackingDataBuffer.Eye.Left.PupilDiameter_MM, mutationData.PupilMutations.SmoothnessMult);
        input.Eye.Left.Gaze = SimpleLerp(input.Eye.Left.Gaze, trackingDataBuffer.Eye.Left.Gaze, mutationData.GazeMutations.SmoothnessMult);

        input.Eye.Right.Openness = SimpleLerp(input.Eye.Right.Openness, trackingDataBuffer.Eye.Right.Openness, mutationData.OpennessMutations.SmoothnessMult);
        input.Eye.Right.PupilDiameter_MM = SimpleLerp(input.Eye.Right.PupilDiameter_MM, trackingDataBuffer.Eye.Right.PupilDiameter_MM, mutationData.PupilMutations.SmoothnessMult);
        input.Eye.Right.Gaze = SimpleLerp(input.Eye.Right.Gaze, trackingDataBuffer.Eye.Right.Gaze, mutationData.GazeMutations.SmoothnessMult);
        */
    }

    public void Initialize() => Reset();

    public void Reset() 
    {
        for (int i = 0; i < smoothingData.Length ; i++)
            smoothingData[i] = SMOOTH_INIT;
    }
    public UnifiedMutationProperty[] GetProperties()
    {
        List<UnifiedMutationProperty> props = new List<UnifiedMutationProperty>();
        int i = 0;
        foreach (float f in smoothingData)
        {
            props.Add(new UnifiedMutationProperty { Name = ((UnifiedExpressions)i).ToString(), Value = f});
            i++;
        }
        return props.ToArray();
    }

    public void SetProperties(UnifiedMutationProperty[] props)
    {
        if (props != null &&
            props[0].Value.GetType() == typeof(float) &&
            (float)props[0].Value <= 1.0f &&
            (float)props[0].Value >= 0.0f)
        {
            for (int i = 0; i < smoothingData.Length; i++)
                smoothingData[i] = (float)props[i].Value;
        }
    }
}