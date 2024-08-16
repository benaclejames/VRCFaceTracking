using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public class Smoothing : TrackingMutation
{
    public class SmoothingData
    {
        public float weight = 0.2f;
        public float[] shapes = new float[(int)UnifiedExpressions.Max + 1];
        public float pupil = 0f;
        public float openness = 0f;
        public float gaze = 0f;
    }

    private SmoothingData smoothingData = new();

    public override string Name => "Unified Smoothing";

    public override string Description => "Default smoothing for VRCFaceTracking expressions.";

    public override MutationPriority Step => MutationPriority.Postprocessor;

    public override bool IsSaved => true;

    public override List<MutationProperty> Properties => new();

    UnifiedTrackingData _trackingDataBuffer = new();

    static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < data.Shapes.Length; i++)
        {
            data.Shapes[i].Weight =
                SimpleLerp(
                    data.Shapes[i].Weight,
                    _trackingDataBuffer.Shapes[i].Weight,
                    smoothingData.shapes[i]
                );
        }

        data.Eye.Left.Openness = SimpleLerp(data.Eye.Left.Openness, _trackingDataBuffer.Eye.Left.Openness, smoothingData.openness);
        data.Eye.Left.PupilDiameter_MM = SimpleLerp(data.Eye.Left.PupilDiameter_MM, _trackingDataBuffer.Eye.Left.PupilDiameter_MM, smoothingData.pupil);
        data.Eye.Left.Gaze = SimpleLerp(data.Eye.Left.Gaze, _trackingDataBuffer.Eye.Left.Gaze, smoothingData.gaze);

        data.Eye.Right.Openness = SimpleLerp(data.Eye.Right.Openness, _trackingDataBuffer.Eye.Right.Openness, smoothingData.openness);
        data.Eye.Right.PupilDiameter_MM = SimpleLerp(data.Eye.Right.PupilDiameter_MM, _trackingDataBuffer.Eye.Right.PupilDiameter_MM, smoothingData.pupil);
        data.Eye.Right.Gaze = SimpleLerp(data.Eye.Right.Gaze, _trackingDataBuffer.Eye.Right.Gaze, smoothingData.gaze);

        _trackingDataBuffer = data;
    }

    public async override Task SaveData(ILocalSettingsService localSettingsService) => 
        await localSettingsService.SaveSettingAsync("VRCFTDefaultSmoothing", smoothingData, true);
    public async override Task LoadData(ILocalSettingsService localSettingsService) => 
        await localSettingsService.ReadSettingAsync("VRCFTDefaultSmoothing", smoothingData, true);
}
