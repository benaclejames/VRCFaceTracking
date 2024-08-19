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
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

public class Calibration : TrackingMutation
{
    [MutationProperty("Calibration Data Points")]
    public static int points = 256;
    [MutationProperty("Delta")]
    public static float delta = 0.05f;
    [MutationProperty("Confidence Bias")]
    public static float confidenceBias = 1f;
    [MutationProperty("Confidence Lerp")]
    public static float confidenceLerp = 0.2f;
    [MutationProperty("Calibration Weight")]
    public static float weight = 0.6f;
    [MutationProperty("Maximum Timeout", 1, 1000)]
    public static int timeoutValue = 1000;

    public class CalibrationParameter
    {
        public int _localIndex;
        public int timeout = timeoutValue;
        public bool finished;
        public float[] dataPoints;
        public float _delta;
        public float mean;
        public float stdDev = 1f;
        public float confidence = 0f;
        private float _lastRecordedValue;

        public CalibrationParameter(int maxDataPoints, float delta)
        {
            dataPoints = new float[maxDataPoints];
            _delta = delta;
            _lastRecordedValue = float.NaN; // Indicates no value recorded yet
        }

        public void UpdateCalibration(float currentValue, ILogger logger, int index)
        {
            if (finished) return;

            if ((float.IsNaN(_lastRecordedValue) || Math.Abs(currentValue - _lastRecordedValue) >= _delta))
            {
                timeout++;
                if (_localIndex < dataPoints.Length)
                {
                    dataPoints[_localIndex++] = currentValue;
                    var newConfidence = 1f-(float)Math.Pow(stdDev, confidenceBias);
                    confidence *= (1 - confidenceLerp) + confidenceLerp * newConfidence;
                }
                else
                {
                    logger.LogInformation($"Finalized calibrating parameter: {(UnifiedExpressions)index}.");
                    confidence = 1f;
                    finished = true;
                }
                _lastRecordedValue = currentValue;
            }
            else if (timeout > 0)
            {
                timeout--;
            }
            else if (timeout == 0)
            {
                logger.LogInformation($"Timeout occured on parameter: {(UnifiedExpressions)index}.");
                finished = true;
            }
        }

        // Calculate mean and standard deviation
        public void CalculateStats()
        {
            mean = dataPoints.Where(v => !float.IsNaN(v)).Average();
            stdDev = (float)Math.Sqrt(dataPoints.Where(v => !float.IsNaN(v))
                                     .Select(v => Math.Pow(v - mean, 2))
                                     .Average());
        }
    }

    public class CalibrationData
    {
        public CalibrationParameter[] Shapes;

        public CalibrationData(int maxParameters)
        {
            Shapes = new CalibrationParameter[maxParameters];
            for (int i = 0; i < maxParameters; i++)
            {
                Shapes[i] = new CalibrationParameter(points, delta);
            }
        }

        public void RecordData(float[] values, ILogger logger)
        {
            for (int i = 0; i < Shapes.Length; i++)
                Shapes[i].UpdateCalibration(values[i], logger, i);
        }

        public void FinalizeCalibration()
        {
            foreach (var parameter in Shapes)
            {
                parameter.CalculateStats();
                // e.g., adjust curvePower, biasStrength, or any other parameters
            }
        }
        public void Clear()
        {
            for (int i = 0; i < Shapes.Length; i++)
            {
                Shapes[i].timeout = timeoutValue;
                Shapes[i].mean = 0f;
                Shapes[i].stdDev = 1f;
                Shapes[i].confidence = 0f;
                Shapes[i].finished = false;
                Shapes[i].dataPoints = new float[points];
                Shapes[i]._localIndex = 0;
                Shapes[i]._delta = delta;
            }
        }

        public bool Calibrating()
        {
            foreach (var shape in Shapes)
            {
                if (!shape.finished)
                    return true;
            }
            return false;
        }
    }

    public CalibrationData calData = new((int)UnifiedExpressions.Max);

    public override string Name => "Calibration";
    public override string Description => "Default VRCFaceTracking calibration that processes raw tracking data into normalized tracking data to better match user expression.";
    public override MutationPriority Step => MutationPriority.Preprocessor;
    public override bool IsSaved => true;
    public override bool IsActive { get; set; } = false;

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (var i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (data.Shapes[i].Weight <= 0.0f)
            {
                continue;
            }

            var calShape = calData.Shapes[i];
            var newShape = data.Shapes[i].Weight * (1-calShape.confidence) + (calShape.confidence) * (data.Shapes[i].Weight / calShape.stdDev);
            data.Shapes[i].Weight = data.Shapes[i].Weight * (1 - weight) + weight * newShape;
        }
    }

    [MutationButton("Initialize Calibration")]
    public void InitializeCalibration()
    {
        Logger.LogInformation("Initialized calibration.");
        StartCalibration();
        Logger.LogInformation("Calibration finalized.");
    }

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
        calData.Clear();

        while (calData.Calibrating())
        {
            var simulatedValues = GetLiveValues();
            calData.RecordData(simulatedValues, Logger);
            calData.FinalizeCalibration();

            Thread.Sleep(100);
        }

        calData.FinalizeCalibration();
    }
}
