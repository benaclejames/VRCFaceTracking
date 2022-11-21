namespace VRCFaceTracking.Params
{
    /// <summary>
    /// Struct that represents a single eye.
    /// </summary>
    public struct UnifiedSingleEyeData
    {
        // Eye data.
        public Vector2 GazeNormalized;
        public float PupilDiameter_MM;
        public float Openness;

        // This is for letting VRCFT know if the sent eye data is usable or accurate.
        public bool Valid;
    }

    /// <summary>
    /// Struct that represents all possible eye data. 
    /// </summary>
    public struct UnifiedEyeData
    {
        public UnifiedSingleEyeData Left, Right, Combined;
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
        public float[] Shapes = new float[(int)UnifiedExpressions.Max + 1];

        /// <summary>
        /// Struct that holds all VRCFaceTracking SRanipal data. 
        /// </summary>
        /// <remarks>
        /// Example of useage:
        /// <code>
        /// // Update JawOpen shape in Expression Data.
        /// LegacyShapes[(int)SRanipal_LipShape_V2.JawOpen] = JawOpen;
        /// </code>
        /// </remarks>
        public float[] LegacyShapes = new float[(int)UnifiedExpressions.Max + 1];
    }
}