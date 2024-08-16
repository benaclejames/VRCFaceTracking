using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

public class Calibration : TrackingMutation
{
    struct CalibrationParameter
    {
        public string Name;
        public float Ceil; // The maximum that the parameter reaches.
        public float Floor; // the minimum that the parameter reaches.
        //public float SigmoidMult; // How much should this parameter be affected by the sigmoid function. This makes the parameter act more like a toggle.
        //public float LogitMult; // How much should this parameter be affected by the logit (inverse of sigmoid) function. This makes the parameter act more within the normalized range.
        public float SmoothnessMult; // How much should this parameter be affected by the smoothing function.
    }

    private struct CalibrationData
    {
        public CalibrationParameter Pupil;
        public CalibrationParameter Gaze;
        public CalibrationParameter Openness;
        public CalibrationParameter[] Shapes;
        public float CalibrationWeight;
        public bool ContinuousCalibration;
    }

    private CalibrationData calData;

    public override string Name => "Unified Calibration";
    public override string Description => "Default VRCFaceTracking calibration that processes raw tracking data into normalized tracking data to better match user expression.";
    public override MutationPriority Step => MutationPriority.Preprocessor;
    public override bool IsSaved => true;

    MutationProperty continuousCalibration = new()
    {
        Name = "Continuous Calibration",
        Value = false,
        Type = MutationPropertyType.CheckBox
    };

    public override List<MutationProperty> Properties => new()
    {
        continuousCalibration,
    };

    public async override Task Initialize(UnifiedTrackingData data) =>
        await InitializeCalibration();
    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (data.Shapes[i].Weight <= 0.0f)
            {
                continue;
            }

            if (calData.CalibrationWeight > 0.0f && data.Shapes[i].Weight > calData.Shapes[i].Ceil) // Calibrator
            {
                calData.Shapes[i].Ceil = SimpleLerp(data.Shapes[i].Weight, calData.Shapes[i].Ceil, calData.CalibrationWeight);
            }

            if (calData.CalibrationWeight > 0.0f && data.Shapes[i].Weight < calData.Shapes[i].Floor)
            {
                calData.Shapes[i].Floor = SimpleLerp(data.Shapes[i].Weight, calData.Shapes[i].Floor, calData.CalibrationWeight);
            }

            data.Shapes[i].Weight = (data.Shapes[i].Weight - calData.Shapes[i].Floor) / (calData.Shapes[i].Ceil - calData.Shapes[i].Floor);
        }
    }

    static T SimpleLerp<T>(T input, T previousInput, float value) => (dynamic)input * (1.0f - value) + (dynamic)previousInput * value;
    public void SetCalibration(float floor = 999.0f, float ceiling = 0.0f)
    {
        // Currently eye data does not get parsed by calibration.
        calData.Pupil.Ceil = ceiling;
        calData.Gaze.Ceil = ceiling;
        calData.Openness.Ceil = ceiling;
            
        calData.Pupil.Floor = floor;
        calData.Gaze.Floor = floor;
        calData.Openness.Floor = floor;

        calData.Pupil.Name = "Pupil";
        calData.Gaze.Name = "Gaze";
        calData.Openness.Name = "Openness";

        calData.Shapes = new CalibrationParameter[(int)UnifiedExpressions.Max + 1];
        for (var i = 0; i < calData.Shapes.Length; i++)
        {
            calData.Shapes[i].Name = ((UnifiedExpressions)i).ToString();
            calData.Shapes[i].Ceil = ceiling;
            calData.Shapes[i].Floor = floor;
        }
    }

    public async Task InitializeCalibration(int durationMs = 3000)
    {
        Logger.LogInformation("Initialized calibration.");

        SetCalibration();

        calData.CalibrationWeight = 0.75f;

        Logger.LogInformation("Calibrating deep normalization for {durationSec}s.", durationMs / 100);
        await Task.Delay(durationMs);

        calData.CalibrationWeight = 0.2f;
        Logger.LogInformation("Fine-tuning normalization. Values will be saved on exit.");
    }

    public async override Task SaveData(ILocalSettingsService localSettingsService) =>
        await localSettingsService.SaveSettingAsync("VRCFTDefaultCalibration", calData, true);
    public async override Task LoadData(ILocalSettingsService localSettingsService) =>
        await localSettingsService.ReadSettingAsync("VRCFTDefaultCalibration", calData, true);
}
