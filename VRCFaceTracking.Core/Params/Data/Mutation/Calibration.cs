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
    [MutationProperty("Data Points")]
#endif
    public static int points = 512; // amount of data. higher is better.
#if DEBUG
    [MutationProperty("Delta")]
#endif
    public static float delta = 0.1f; // prevents noisy or unintended data from being included in data set
    [MutationProperty("Calibration Deviation", -1f, 1f)]
    public static float deviationBias = 0f;

    public class CalibrationParameter
    {
        public string name;
        private int _localIndex;
        private int _fixedIndex;
        private bool finished;
        private float[] dataPoints = new float[points];
        private float[] zScores = new float[points];
        public float mean = 0f;
        public float stdDev = 0f;
        public float variance = 0f;
        public float confidence = 0f;
        public float progress;
        private float _currentStep;
        public float max;

        public CalibrationParameter(string name)
        {
            this.name = name;
        }

        public void UpdateCalibration(float currentValue, ILogger logger, float dT)
        {
            if (dataPoints == null)
                dataPoints = new float[points];

            var difference = Math.Abs(currentValue - _currentStep);
            if ((float.IsNaN(_currentStep) || difference >= delta * dT))
            {
                if (_fixedIndex < dataPoints.Length)
                {
                    _fixedIndex++;
                    progress = _fixedIndex / (float)dataPoints.Length;
                }
                else if (!finished)
                {
                    logger.LogInformation($"Data fully saturated: {name}.");
                    finished = true;
                }
                dataPoints[_localIndex] = currentValue;
                _localIndex = (_localIndex + 1) % dataPoints.Length;
                CalculateStats();
            }
            _currentStep = ClampStep(currentValue);
        }

        private float ClampStep(float value) => (float)Math.Floor(value / delta) * delta; 

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
                var _variance = (_stdDev / _mean);
                var _varianceConf = 1f - Math.Abs((_variance) - 1f);
                var _minimumStdMean = (float)Math.Pow(6.8f * _mean * _stdDev, 0.35f);
                var _confidence = ZConfidence(dataPoints, _mean, _stdDev) * progress * _varianceConf * _minimumStdMean;
                if (_confidence > confidence)
                {
                    if (!float.IsNaN(_mean))
                        mean = _mean;
                    if (!float.IsNaN(_stdDev))
                        stdDev = _stdDev;
                    if (!float.IsNaN(_confidence))
                        confidence = _confidence;
                    if (!float.IsNaN(_variance))
                        variance = _variance;
                }
            }
        }

        public float CalculateParameter(float currentValue, float k)
        {
            var adjustedMax = mean + k * stdDev;
            var curvedValue = (float)Math.Pow(currentValue, CurveAdjustedRange(adjustedMax));
            return confidence * Math.Clamp(curvedValue, 0.0f, 1.0f) + (1f - confidence) * currentValue;
        }

        public bool IsFinished() => finished;
    }

    public class CalibrationData
    {
        public CalibrationParameter[] Shapes = new CalibrationParameter[(int)UnifiedExpressions.Max];

        public void RecordData(float[] values, ILogger logger, int ms)
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                if (Shapes[i] == null)
                    Shapes[i] = new CalibrationParameter(((UnifiedExpressions)i).ToString());
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

        public bool Calibrating()
        {
            foreach (var shape in Shapes)
            {
                if (!shape.IsFinished())
                    return true;
            }
            return false;
        }
    }

    public CalibrationData calData = new();

    public override string Name => "Calibration";
    public override string Description => "Processes tracking data to better match user expression.";
    public override MutationPriority Step => MutationPriority.Preprocessor;
    public override bool IsSaved => true;
    public override bool IsActive { get; set; } = false;

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (calData.Shapes[i] == null)
                calData.Shapes[i] = new CalibrationParameter(((UnifiedExpressions)i).ToString());
            if (data.Shapes[i].Weight <= 0.0f)
            {
                continue;
            }

            data.Shapes[i].Weight = calData.Shapes[i].CalculateParameter(data.Shapes[i].Weight, deviationBias);
        }
    }

    [MutationButton("Initialize Calibration")]
    public void InitializeCalibration()
    {
        Logger.LogInformation("Initializing calibration.");
        calData.Clear();
        StartCalibration();
            Logger.LogInformation("Calibration finalized.");
    }

#if DEBUG
    [MutationButton("Read Values")]
    public void ReadValues()
    {
        for (int i = 0; i < calData.Shapes.Length; i++)
        {
            Logger.LogInformation($"{(UnifiedExpressions)i}" +
                                  $"\n  progress: {calData.Shapes[i].progress}" +
                                  $"\n  mean: {calData.Shapes[i].mean}" +
                                  $"\n  stdDev: {calData.Shapes[i].stdDev}" +
                                  $"\n  confidence: {calData.Shapes[i].confidence}" +
                                  $"\n  variance: {calData.Shapes[i].variance}" +
                                  $"\n  raw value: {UnifiedTracking.Data.Shapes[i].Weight}" +
                                  $"\n  weighted value: {calData.Shapes[i].CalculateParameter(UnifiedTracking.Data.Shapes[i].Weight, deviationBias)}");
        }
    }
#endif

    private float[] GetLiveValues()
    {
        var shapes = UnifiedTracking.Data.Shapes;
        float[] values = new float[shapes.Length];
        
        for (int i = 0; i < shapes.Length; i++)
        {
            values[i] = shapes[i].Weight;
        }
        
        return values;
    }

    readonly int updateMs = 10;

    private void StartCalibration()
    {
        while (calData.Calibrating())
        {
            var simulatedValues = GetLiveValues();
            calData.RecordData(simulatedValues, Logger, updateMs);
            Thread.Sleep(updateMs);
        }
    }
}
