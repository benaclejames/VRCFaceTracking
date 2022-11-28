using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace VRCFaceTracking.Params
{
    /// <summary>
    /// Struct that represents a single eye.
    /// </summary>
    public struct UnifiedSingleEyeData
    {
        // Eye data.
        public Vector2 Gaze;
        public float PupilDiameter_MM;
        public float Openness;
    }

    /// <summary>
    /// Struct that represents all possible eye data. 
    /// </summary>
    public class UnifiedEyeData
    {
        public UnifiedSingleEyeData Left, Right;
        private float _minDilation = 999f;
        private float _maxDilation = 0f;
        public UnifiedSingleEyeData Combined()
        {
            if ((Left.PupilDiameter_MM + Left.PupilDiameter_MM) / 2.0f < _minDilation)
                _minDilation = (Left.PupilDiameter_MM + Left.PupilDiameter_MM) / 2.0f;

            if ((Left.PupilDiameter_MM + Left.PupilDiameter_MM) / 2.0f > _maxDilation)
                _maxDilation = (Left.PupilDiameter_MM + Left.PupilDiameter_MM) / 2.0f;

            return new UnifiedSingleEyeData
            {
                Gaze = (Left.Gaze + Right.Gaze) / 2.0f,
                Openness = (Left.Openness + Right.Openness) / 2.0f,
                PupilDiameter_MM = (((Left.PupilDiameter_MM + Left.PupilDiameter_MM) / 2.0f) - _minDilation) / (_maxDilation - _minDilation),
            };
        }
    }

    public struct UnifiedExpressionShape
    {
        public float Weight;
        public float CalibrationMultiplier;
        public float SmoothingMultiplier;
    }

    /// <summary>
    /// Struct that represents the data accessible by modules and parameters used with VRCFaceTracking.
    /// </summary>
    public class UnifiedExpressionsData
    {
        /// <summary>
        /// Struct that holds all possible eye data. 
        /// </summary>
        public UnifiedEyeData Eye = new UnifiedEyeData();

        /// <summary>
        /// Struct that holds all Unified Expression data. 
        /// </summary>
        /// <remarks>
        /// Example of useage:
        /// <code>
        /// // Update JawOpen shape in Expression Data.
        /// Shapes[(int)UnifiedExpression.JawOpen] = JawOpen;
        /// </code>
        /// </remarks>
        public UnifiedExpressionShape[] Shapes = new UnifiedExpressionShape[(int)UnifiedExpressions.Max + 1];
    }
}