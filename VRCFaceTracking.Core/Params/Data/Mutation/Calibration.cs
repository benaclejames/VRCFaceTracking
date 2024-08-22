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
    public static int points = 256;
#if DEBUG
    [MutationProperty("Delta")]
#endif
    public static float delta = 0.02f;
#if DEBUG
    [MutationProperty("Confidence Bias", 0.5f, 2f)]
#endif
    public static float confidenceBias = 1f;
    [MutationProperty("Deviation Bias", 0f, 5f)]
    public static float deviationBias = 2f;
    [MutationProperty("Calibration Weight")]
    public static float weight = 0.6f;
#if DEBUG
    [MutationProperty("Maximum Timeout", 1, 1000)]
#endif
    public static int timeoutValue = 1000;
    [MutationProperty("Clear Data Upon Calibration")]
    public static bool resetCalibration = false;

    public class CalibrationParameter
    {
        private int _localIndex;
        public bool finished;
        public float timeout = timeoutValue;
        private float[] dataPoints = new float[points];
        public float mean = 0.5f;
        public float stdDev = 0.5f;
        public float confidence = 0f;
        private float _lastRecordedValue;

        public CalibrationParameter()
        {
        }

        public void UpdateCalibration(float currentValue, ILogger logger, int index)
        {
            if (finished) return;
            if (dataPoints == null)
                dataPoints = new float[points];

            if ((float.IsNaN(_lastRecordedValue) || Math.Abs(currentValue - _lastRecordedValue) >= delta))
            {
                timeout++;
                if (_localIndex < dataPoints.Length)
                {
                    dataPoints[_localIndex++] = currentValue;
                }
                else
                {
                    logger.LogInformation($"Finalized calibrating parameter: {(UnifiedExpressions)index}.");
                    finished = true;
                }
                confidence = 1f - (float)Math.Pow(stdDev, confidenceBias) * ((float)_localIndex / dataPoints.Length);
                _lastRecordedValue = currentValue;
            }
            else timeout--;
        }

        // Calculate mean and standard deviation
        public void CalculateStats()
        {
            var _mean = dataPoints.Where(v => !float.IsNaN(v)).Average();
            mean = _mean;
            stdDev = (float)Math.Sqrt(dataPoints.Where(v => !float.IsNaN(v))
                                     .Select(v => Math.Pow(v - _mean, 2))
                                     .Average());
        }

        public float CalculateParameter(float currentValue, float k)
        {
            float upperBound = mean + k * stdDev;
            float scaledValue = currentValue / upperBound;
            return confidence * Math.Clamp(scaledValue, 0.0f, 1.0f) + (1f-confidence) * currentValue;
        }

        public bool IsFinished() => finished;

        public void Reset()
        {
            mean = 0f;
            stdDev = 1f;
            confidence = 0f;
            timeout = timeoutValue;
            finished = false;
            dataPoints = new float[points];
            _localIndex = 0;
        }

        public void Reinstate()
        {
            timeout = timeoutValue;
            finished = false;
            confidence = 0f;
        }
    }

    public class CalibrationData
    {
        public CalibrationParameter[] Shapes = new CalibrationParameter[(int)UnifiedExpressions.Max];

        public void RecordData(float[] values, ILogger logger)
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                if (Shapes[i] == null)
                    Shapes[i] = new CalibrationParameter();
                Shapes[i].UpdateCalibration(values[i], logger, i);
                Shapes[i].CalculateStats();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i].Reset();
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
        public void Reinstate()
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                if (Shapes[i] == null)
                    Shapes[i] = new CalibrationParameter();
                Shapes[i].Reinstate();
            }
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
                calData.Shapes[i] = new CalibrationParameter();
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
        Logger.LogInformation("Initialized calibration.");
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
                                      $"\n         mean: {calData.Shapes[i].mean }" +
                                      $"\n         stdDev: {calData.Shapes[i].stdDev}" +
                                      $"\n         confidence: {calData.Shapes[i].stdDev}");
            }
    }
#endif
    private float[] GetLiveValues()
    {
        // Access the live data from UnifiedTracking.Data.Shapes
        var shapes = UnifiedTracking.Data.Shapes;
        float[] values = new float[shapes.Length];
        
        for (int i = 0; i < shapes.Length; i++)
        {
            values[i] = shapes[i].Weight; // Assuming Weight is the value to be calibrated
        }
        
        return values;
    }

    private void StartCalibration()
    {
        if (resetCalibration)
            calData.Clear();
        else calData.Reinstate();
        while (calData.Calibrating())
        {
            var simulatedValues = GetLiveValues();
            calData.RecordData(simulatedValues, Logger);
            Thread.Sleep(100);
        }
    }
}
