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
        public float _minDilation = 999f;
        public float _maxDilation = 0f;
        public float GazeSmoothness;
        public float PupilDiameterSmoothness;
        public float OpennessSmoothness;

        /// <summary>
        /// Creates relevant data that combines the Left and Right single eye datas into a combined single eye data.
        /// </summary>
        /// <returns>Relevant combined Left and Right Eye data</returns>
        /// <remarks>
        /// Pupil Dilation is averaged between both the Left and Right eye datas, and also normalizes the range between 0.0 - 1.0. 
        /// The combined eye gaze is based off of the validity of the eye data; Both eyes being valid will return the averaged vectors between the two,
        /// If only one is valid then the valid eye will control the gaze vector.
        /// </remarks>
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

    /// <summary>
    /// Container of information pertaining to a singular Unified Expression shape.
    /// </summary>
    public struct UnifiedExpressionShape
    {
        /// <summary>
        /// Value that contains the specified Unified Expression raw value.
        /// </summary>
        [JsonIgnore] public float Weight;

        /// <summary>
        /// Value that contains an adjusted parameter value that drives the specified Unified Expression output.
        /// </summary>
        /// <remarks>This value is a result of the original Weight being mutated into this value.</remarks>
        [JsonIgnore] internal float AdjustedWeight;

        /// <summary>
        /// How much the Weight will be multiplied by the Mutator system to be within bounds of 0.0 - 1.0.
        /// </summary>
        public float CalibrationMult;

        /// <summary>
        /// How much the Weight will be smoothed out by the Mutator system using the current and previous Weights.
        /// </summary>
        public float SmoothnessMult;
    }

    /// <summary>
    /// All data that is accessible by modules and is output to parameters.
    /// </summary>
    public class UnifiedTrackingData
    {
        /// <summary>
        /// Container of all relevant Unified raw eye data. 
        /// </summary>
        public UnifiedEyeData Eye = new UnifiedEyeData();
        
        /// <summary>
        /// Container of all updated Unified eye data. 
        /// </summary>
        internal UnifiedEyeData AdjustedEye = new UnifiedEyeData();

        /// <summary>
        /// Container of all Unified Expression expression data. 
        /// </summary>
        /// <remarks>
        /// Example of useage:
        /// <code>
        /// // Update JawOpen shape in Expression Data.
        /// Shapes[(int)UnifiedExpression.JawOpen].Weight = JawOpen;
        /// </code>
        /// </remarks>
        public UnifiedExpressionShape[] Shapes = new UnifiedExpressionShape[(int)UnifiedExpressions.Max + 1];

        public void ResetCalibration()
        {
            for (int i = 0; i < Shapes.Length; i++)
                Shapes[i].CalibrationMult = 0.0f;
        }
    }
}