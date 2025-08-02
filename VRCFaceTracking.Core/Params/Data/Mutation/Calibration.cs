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
    [MutationProperty("[DEBUG] Data Points")]
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
    [MutationProperty("Calibration Deviation", -1f, 1f)]
    public float deviationBias = 0f;

    public class CalibrationParameter
    {
        public string name;
        private int _rollingIndex;
        private int _fixedIndex;
        private bool finished;
        private float[] dataPoints = new float[points];
        private float[] zScores = new float[points];
        public float mean = 0f;
        public float stdDev = 0f;
        public float variance = 0f;
        public float confidence = 0f;
        public float maxConfidence = 0f;
        public float progress;
        private float _currentStep;
        public float max;

        public CalibrationParameter(string name)
        {
            this.name = name;
        }

        public void UpdateCalibration(float currentValue, ILogger logger, float dT)
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
                    logger.LogInformation($"Data saturated window: {name}.");
                    finished = true;
                }
                dataPoints[_rollingIndex] = currentValue;
                _rollingIndex = (_rollingIndex + 1) % dataPoints.Length;
                CalculateStats();
            }
            _currentStep = ClampStep(currentValue, sDelta * dT);
        }

        private float ClampStep(float value, float factor) => (float)Math.Floor(value / factor) * factor; 

        private float StdDev(float[] data, float mean) =>
        (float)Math.Sqrt(data.Take((int)(_fixedIndex - 1)).Where(v => !float.IsNaN(v))
                   .Select(v => Math.Pow(v - mean, 2))
                   .Average());
        private float Mean(float[] data) =>
            data.Take((int)(_fixedIndex - 1)).Where(v => !float.IsNaN(v)).Average();

        private float ZConfidence(float[] data, float mean, float stdDev)
        {
            for(int i = 0; i <= _fixedIndex-1; i++)
                zScores[i] = (data[i] - mean) / stdDev;
            var score = 1f - Math.Min(1f, Math.Abs(zScores.Average()));
            if (float.IsNaN(score))
                return 0f;
            return score;
        }

        private float CurveAdjustedRange(float value) =>
            1f-((1f - value) / (1f + value));

        public void CalculateStats()
        {
            if (_fixedIndex >= 0.1f * dataPoints.Length)
            {
                var _mean = Mean(dataPoints);
                var _stdDev = StdDev(dataPoints, _mean);
                var _variance = (float)Math.Pow(Math.Max(0f, 1f - (float)Math.Abs((1.7241379310337f * _stdDev) - _mean)), 1f);
                var _stdDevLimit = Math.Pow(1f - Math.Max(0, _stdDev - 0.29f),3f);
                var _meanLimit = 1f - Math.Max(0, _mean - 0.5f);
                var _meanPusher = Math.Pow(2*_mean, 0.2f);
                
                var _confidence = (float)Math.Max(0f, Math.Min(1f, 
                    _variance *
                    _stdDevLimit *
                    _meanLimit *
                    _meanPusher
                ));
                
                if (_confidence >= maxConfidence - cDelta)
                {
                    var _lerp = 1f - (float)Math.Pow(confidence, 2f); // weighs new stats less the more confident we are.
                    if (!float.IsNaN(_mean))
                        mean = _mean * _lerp + mean * (1f-_lerp);
                    if (!float.IsNaN(_stdDev))
                        stdDev = _stdDev * _lerp + stdDev * (1f-_lerp);
                    if (!float.IsNaN(_confidence))
                        confidence = _confidence * _lerp + confidence * (1f-_lerp);
                    if (confidence > maxConfidence)
                        maxConfidence = maxConfidence * _lerp + confidence * (1f-_lerp);
                    if (!float.IsNaN(_variance))
                        variance = _variance * _lerp + variance * (1f - _lerp);
                }
            }
        }

        public float CalculateParameter(float currentValue, float k)
        {
            if (float.IsNaN(currentValue)) 
                return currentValue;
            var adjustedMax = mean + k * stdDev;
            var curvedValue = (float)Math.Pow(currentValue, CurveAdjustedRange(adjustedMax));
            var lerp = (float)(confidence * (1f / (1f + Math.Pow(2, -200f * currentValue + 7f)) * (float)Math.Max(0f, Math.Pow(Math.Abs(3.34f*stdDev-1f), 0.2f))));
            var adjustedValue = lerp * Math.Clamp(curvedValue, 0.0f, 1.0f) + (1f - lerp) * currentValue;
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
                    Shapes[i] = new CalibrationParameter(((UnifiedExpressions)i).ToString());
        }

        public void RecordData(float[] values, ILogger logger, int ms)
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i].UpdateCalibration(values[i], logger, ms/1000f);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i] = new CalibrationParameter(((UnifiedExpressions)i).ToString());
            }
        }
    }

    public CalibrationData calData;

    public override string Name => "Calibration";
    public override string Description => "Processes tracking data to better match user expression.";
    public override MutationPriority Step => MutationPriority.Preprocessor;
    public override bool IsSaved => true;
    public override bool IsActive { get; set; } = true;

    public override void Initialize(UnifiedTrackingData data) => calData ??= new();

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            calData.Shapes[i].UpdateCalibration(data.Shapes[i].Weight, Logger, 100f/1000f);
            data.Shapes[i].Weight = calData.Shapes[i].CalculateParameter(data.Shapes[i].Weight, deviationBias);
        }
    }

#if DEBUG
    [MutationButton("[DEBUG] Log Data")]
    public void LogData()
    {
        Logger.LogInformation("Logging Calibration data:" +
                             $" delta: {sDelta}" + 
                             $" points: {points}" + 
                             $" devationBias: {deviationBias}");
        for (int i = 0; i < calData.Shapes.Length; i++)
        {
            Logger.LogInformation($"{(UnifiedExpressions)i}" +
                                  $"\n  progress: {calData.Shapes[i].progress}" +
                                  $"\n  mean: {calData.Shapes[i].mean}" +
                                  $"\n  stdDev: {calData.Shapes[i].stdDev}" +
                                  $"\n  confidence: {calData.Shapes[i].confidence}" +
                                  $"\n  maxConfidence: {calData.Shapes[i].maxConfidence}" +
                                  $"\n  variance: {calData.Shapes[i].variance}" +
                                  $"\n  raw value: {UnifiedTracking.Data.Shapes[i].Weight}" +
                                  $"\n  weighted value: {calData.Shapes[i].CalculateParameter(UnifiedTracking.Data.Shapes[i].Weight, deviationBias)}");
        }
    }
#endif

    [MutationButton("Clear Calibration")]
    public void ClearData() => calData.Clear();
}
