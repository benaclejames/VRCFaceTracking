using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

public class Calibration : TrackingMutation
{
#if DEBUG
    [MutationProperty("[DEBUG] Window Size")]
#endif
    public static int points = 64;
#if DEBUG
    [MutationProperty("[DEBUG] Step Delta")]
#endif
    public static float sDelta = 0.15f; // prevents noisy or unintended data from being included in data set
#if DEBUG
    [MutationProperty("[DEBUG] Calibration Delta")]
#endif
    public static float cDelta = 0.1f; // prevents noisy or unintended data from being included in data set
    [MutationProperty("Calibration Blend", 0f, 1f)]
    public float calibrationBlend = 1f;

    [MutationProperty("Continuous Calibration")]
    public bool continuousCalibration = false;

    public class CalibrationParameter
    {
        public string name;
        private int _rollingIndex;
        private int _fixedIndex;
        private bool finished;
        private float[] dataPoints = new float[points];
        public float progress;
        private float _currentStep;
        public float max;

        public CalibrationParameter(){}

        public void UpdateCalibration(float currentValue, bool continuous, ILogger logger, float dT)
        {
            var difference = Math.Abs(currentValue - _currentStep);
            if ((float.IsNaN(_currentStep) || difference >= sDelta * dT))
            {
                if (_fixedIndex < dataPoints.Length)
                {
                    _fixedIndex++;
                    progress = _fixedIndex / (float)dataPoints.Length;
                }
                else if (!finished)
                {
                    logger.LogDebug($"Data saturated window: {name}.");
                    finished = true;
                }
                dataPoints[_rollingIndex] = currentValue;
                if (!finished || (finished && continuous))
                {
                    _rollingIndex = (_rollingIndex + 1) % dataPoints.Length;
                    CalculateStats();
                }
            }
            _currentStep = ClampStep(currentValue, sDelta * dT);
        }

        private float ClampStep(float value, float factor) => (float)Math.Floor(value / factor) * factor; 

        public void CalculateStats()
        {
            if (_fixedIndex >= 0.1f * dataPoints.Length)
            {
                var _max = dataPoints.Max();

                if (_max > max)
                    max = _max;
            }
        }

        private float Normalize(float currentValue) =>
            currentValue / max;

        public float CalculateParameter(float currentValue, float k)
        {
            if (float.IsNaN(currentValue)) 
                return currentValue;

            var confidence = k * progress;
            var adjustedValue = confidence * Normalize(currentValue) + (1 - confidence) * currentValue;

            if (float.IsNaN(adjustedValue))
                return currentValue;

            return adjustedValue;
        }
    }

    public class CalibrationData
    {
        public CalibrationParameter[] Shapes;

        public CalibrationData()
        {
            Shapes ??= new CalibrationParameter[(int)UnifiedExpressions.Max];
            for (int i = 0; i < Shapes.Length; i++)
                if (Shapes[i] == null)
                    Shapes[i] = new CalibrationParameter 
                    { 
                        name = ((UnifiedExpressions)i).ToString(),
                        max = 0f,
                    };
        }

        public void RecordData(float[] values, bool continuous, ILogger logger, int ms)
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i].UpdateCalibration(currentValue: values[i], 
                                            continuous: continuous, 
                                            logger: logger, 
                                            dT: ms/1000f);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i] = new CalibrationParameter
                {
                    name = ((UnifiedExpressions)i).ToString(),
                    max = 0f,
                };
            }
        }
    }

    public CalibrationData calData;

    public override string Name => "Calibration";
    public override string Description => "Normalizes tracking data.";
    public override MutationPriority Step => MutationPriority.Preprocessor;
    public override bool IsSaved => true;
    public override bool IsActive { get; set; } = false;

    public override void Initialize(UnifiedTrackingData data) => calData ??= new();

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            calData.Shapes[i].UpdateCalibration(currentValue: data.Shapes[i].Weight, 
                                                continuous: continuousCalibration,
                                                logger: Logger, 
                                                dT: 100f/1000f);

            data.Shapes[i].Weight = calData.Shapes[i].CalculateParameter(data.Shapes[i].Weight, calibrationBlend);
        }
    }

#if DEBUG
    [MutationButton("[DEBUG] Log Data")]
    public void LogData()
    {
        Logger.LogInformation("Logging Calibration data:" +
                             $" delta: {sDelta}" +
                             $" points: {points}");
        for (int i = 0; i < calData.Shapes.Length; i++)
        {
            Logger.LogInformation($"{(UnifiedExpressions)i}" +
                                  $"\n  max value: {calData.Shapes[i].max}" +
                                  $"\n  raw value: {UnifiedTracking.Data.Shapes[i].Weight}" +
                                  $"\n  weighted value: {calData.Shapes[i].CalculateParameter(UnifiedTracking.Data.Shapes[i].Weight, calibrationBlend)}");
        }
    }
#endif

    [MutationButton("Reset Calibration")]
    public void ClearData() => calData.Clear();
}
