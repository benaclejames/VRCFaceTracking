using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.Core.Params.Data
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
        public float _maxDilation, _minDilation = 999f;
        public float _leftDiameter, _rightDiameter;

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
            var averageDilation = (Left.PupilDiameter_MM + Right.PupilDiameter_MM) / 2.0f;
            var leftDiff = Math.Abs(_leftDiameter - Left.PupilDiameter_MM) > 0f;
            var rightDiff = Math.Abs(_rightDiameter - Right.PupilDiameter_MM) > 0f;

            if (leftDiff || rightDiff)
            {
                if (averageDilation > _maxDilation)
                    _maxDilation = averageDilation;
                else if (averageDilation < _minDilation)
                    _minDilation = averageDilation;
            }
            if (leftDiff)
                _leftDiameter = Left.PupilDiameter_MM;
            if (rightDiff)
                _rightDiameter = Right.PupilDiameter_MM;

            var normalizedDilation = (averageDilation - _minDilation) / (_maxDilation - _minDilation);

            return new UnifiedSingleEyeData
            {
                Gaze = (Left.Gaze + Right.Gaze) / 2.0f,
                Openness = (Left.Openness + Right.Openness) / 2.0f,
                PupilDiameter_MM = float.IsNaN(normalizedDilation) ? 0.5f : normalizedDilation,
            };
        }
        public void CopyPropertiesOf(UnifiedEyeData data)
        {
            data.Combined();

            this.Left = data.Left;
            this.Right = data.Right;
            this._maxDilation = data._maxDilation;
            this._minDilation = data._minDilation;
            this._rightDiameter = data._rightDiameter;
            this._leftDiameter = data._leftDiameter;
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
        public float Weight;
    }

    public struct UnifiedHeadData
    {
        /// <summary>
        /// Contains head rotation and position data.
        /// Head coordinate system should be *LEFT handed, Y-up* (matching Unity coordinate system)
        /// Head rotation values should be normalized to a [-1, 1] range representing -90d to 90d rotation (0 meaning facing directly forwards)
        ///     Yaw - Y axis rotation (positive values = looking right)
        ///     Pitch - X axis rotation (positive values = looking down)
        ///     Roll - Z axis rotation (positive values = side-tilt left)
        /// Head position values should be normalized / capped to a [-1, 1] range and represent deviation from a set user origin point in meters
        ///     The normalized values should be *approximately* represent a 1x1x1 meter movement region about the user's origin point 
        ///     (i.e. HeadPosX = 1 means the user is 0.5m to their right from their starting point, HeadPosX = -1 the user is 0.5m to their left)
        /// </summary>
        public float HeadYaw;
        public float HeadPitch;
        public float HeadRoll;

        public float HeadPosX;
        public float HeadPosY;
        public float HeadPosZ;
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
        /// Container of all Unified Expression expression data. 
        /// </summary>
        /// <remarks>
        /// Example of usage:
        /// <code>
        /// // Update JawOpen shape in Expression Data.
        /// Shapes[(int)UnifiedExpression.JawOpen].Weight = JawOpen;
        /// </code>
        /// </remarks>
        public UnifiedExpressionShape[] Shapes = new UnifiedExpressionShape[(int)UnifiedExpressions.Max + 1];

        /// <summary>
        /// Container of head pose data.
        /// </summary>
        public UnifiedHeadData Head = new UnifiedHeadData();

        public void CopyPropertiesOf(UnifiedTrackingData data)
        {
            Eye.CopyPropertiesOf(data.Eye);
            for (int i = 0; i < Shapes.Length; i++)
                Shapes[i].Weight = data.Shapes[i].Weight;
            
            Head.HeadYaw   = data.Head.HeadYaw;
            Head.HeadPitch = data.Head.HeadPitch;
            Head.HeadRoll  = data.Head.HeadRoll;
            Head.HeadPosX  = data.Head.HeadPosX;
            Head.HeadPosY  = data.Head.HeadPosY;
            Head.HeadPosZ  = data.Head.HeadPosZ; 
        }
    }
}