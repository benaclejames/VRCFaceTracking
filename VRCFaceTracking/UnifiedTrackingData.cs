using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Eye;
using VRCFaceTracking.Params.LipMerging;
using Vector2 = VRCFaceTracking.Params.Vector2;

namespace VRCFaceTracking
{
    // Represents a single eye, can also be used as a combined eye
    public struct Eye
    {
        public Vector2 Look;
        public float Openness;
        public float Widen, Squeeze;

        
        public void Update(SingleEyeData eyeData, SingleEyeExpression? expression = null)
        {
            if (eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                Look = eyeData.gaze_direction_normalized.Invert();

            Openness = eyeData.eye_openness;
            
            if (expression == null) return; // This is null when we use this as a combined eye, so don't try read data from it
            
            Widen = expression.Value.eye_wide;
            Squeeze = expression.Value.eye_squeeze;
        }
    }
    
    public class EyeTrackingData
    {
        // Camera Data
        public (int x, int y) ImageSize;
        public byte[] ImageData;
        public bool SupportsImage;
        
        public Eye Left, Right, Combined;
        
        // SRanipal Exclusive
        public float EyesDilation;
        private float _maxDilation, _minDilation;
        
        // Custom parameter
        public float EyesPupilDiameter;
        public float ConvergencePlaneDistance20M;
        public float ConvergencePlaneDistance10M;
        public float ConvergencePlaneDistance5M;
        public float ConvergencePlaneDistance2M;
        public float ConvergencePlaneDistance1M;
        public float ConvergencePlaneDistanceRawM;
        public float ConvergencePointDistance20M;
        public float ConvergencePointDistance10M;
        public float ConvergencePointDistance5M;
        public float ConvergencePointDistance2M;
        public float ConvergencePointDistance1M;
        public float ConvergencePointDistanceRawM;

        public void UpdateData(EyeData_v2 eyeData)
        {
            float dilation = 0;
            
            if (eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.right.pupil_diameter_mm;
                UpdateMinMaxDilation(eyeData.verbose_data.right.pupil_diameter_mm);
            }
            else if (eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                dilation = eyeData.verbose_data.left.pupil_diameter_mm;
                UpdateMinMaxDilation(eyeData.verbose_data.left.pupil_diameter_mm);
            }

            Left.Update(eyeData.verbose_data.left, eyeData.expression_data.left);
            Right.Update(eyeData.verbose_data.right, eyeData.expression_data.right);
            
            Combined.Update(eyeData.verbose_data.combined.eye_data);
            // Fabricate missing combined eye data
            Combined.Widen = (Left.Widen + Right.Widen) / 2;
            Combined.Squeeze = (Left.Squeeze + Right.Squeeze) / 2;
            
            if (dilation != 0)
            {
                EyesDilation = (dilation - _minDilation) / (_maxDilation - _minDilation);
                EyesPupilDiameter = dilation > 10 ? 1 : dilation / 10;
            }

            if (
                eyeData.verbose_data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                && eyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
            )
            {
                UpdateEyeConvergence(eyeData);
            }
        }

        private void UpdateEyeConvergence(EyeData_v2 eyeData)
        {
            // In radians
            var leftGazeAngle = -Math.Asin(eyeData.verbose_data.left.gaze_direction_normalized.x);
            var rightGazeAngle = Math.Asin(eyeData.verbose_data.right.gaze_direction_normalized.x);
            
            // Form the base of the triangle
            var leftComp = Math.PI / 2 - leftGazeAngle;
            var rightComp = Math.PI / 2 - rightGazeAngle;
            
            // The gaze angles are based on the gaze origin, located at the center of the cornea sphere.
            // These cornea spheres move, and both get closer to each other when the eyes converge.
            // This is effectively the dynamic IPD needed for these calculations, rather than the HMD lenses IPD.
            var dynamicIpdMillimeters = eyeData.verbose_data.left.gaze_origin_mm.x - eyeData.verbose_data.right.gaze_origin_mm.x;

            if (leftComp + rightComp >= Math.PI)
            {
                // Either gazing at infinite distance, or diverging (strabismus)
                ConvergencePlaneDistance20M = 1f;
                ConvergencePlaneDistance10M = 1f;
                ConvergencePlaneDistance5M = 1f;
                ConvergencePlaneDistance2M = 1f;
                ConvergencePlaneDistance1M = 1f;
                ConvergencePlaneDistanceRawM = 10000f;
                ConvergencePointDistance20M = 1f;
                ConvergencePointDistance10M = 1f;
                ConvergencePointDistance5M = 1f;
                ConvergencePointDistance2M = 1f;
                ConvergencePointDistance1M = 1f;
                ConvergencePointDistanceRawM = 10000f;
            }
            else
            {
                // Basic trigonometry
                // - Calculate the left side of the triangle where leftComp, rightComp, and IPD form the base.
                var leftSideMillimetres = Math.Sin(rightComp) * dynamicIpdMillimeters / Math.Sin(Math.PI - leftComp - rightComp);

                // Handle two different interpretations of convergence distance. This is more relevant for objects reaching into the intimate space.
                // (In both cases, the convergence distance is not factored by looking up, straight, or down)
                {
                    // # Distance to the convergence plane (triangle height)
                    // This convergence distance corresponds to the distance between the convergence point
                    // and an infinite line passing through the two gaze origins (note: gaze origin is not the center of eyeballs).

                    // - Calculate the height of the right triangle where leftComp is the angle opposite to the convergence distance
                    var convergenceDistanceMillimetres = leftSideMillimetres * Math.Sin(leftComp);

                    var convergenceDistanceMetres = (float)convergenceDistanceMillimetres / 1000f;
                    ConvergencePlaneDistance20M = Saturate(convergenceDistanceMetres / 20f);
                    ConvergencePlaneDistance10M = Saturate(convergenceDistanceMetres / 10f);
                    ConvergencePlaneDistance5M = Saturate(convergenceDistanceMetres / 5f);
                    ConvergencePlaneDistance2M = Saturate(convergenceDistanceMetres / 2f);
                    ConvergencePlaneDistance1M = Saturate(convergenceDistanceMetres / 1f);
                    ConvergencePlaneDistanceRawM = convergenceDistanceMetres;
                }
                {
                    // # Distance to the convergence point (length of median line segment)
                    // This convergence distance corresponds to the distance between the convergence point
                    // and the point between the two gaze origins (also known as combined gaze origin).
                    
                    // - Calculate the right side of the triangle where leftComp, rightComp, and IPD form the base.
                    var rightSideMillimetres = Math.Sin(leftComp) * dynamicIpdMillimeters / Math.Sin(Math.PI - leftComp - rightComp);
                    // - The median length is the average of the two triangle sides
                    var convergenceDistanceMillimetres = (leftSideMillimetres + rightSideMillimetres) / 2;

                    var convergenceDistanceMetres = (float)convergenceDistanceMillimetres / 1000f;
                    ConvergencePointDistance20M = Saturate(convergenceDistanceMetres / 20f);
                    ConvergencePointDistance10M = Saturate(convergenceDistanceMetres / 10f);
                    ConvergencePointDistance5M = Saturate(convergenceDistanceMetres / 5f);
                    ConvergencePointDistance2M = Saturate(convergenceDistanceMetres / 2f);
                    ConvergencePointDistance1M = Saturate(convergenceDistanceMetres / 1f);
                    ConvergencePointDistanceRawM = convergenceDistanceMetres;
                }
            }
        }

        private float Saturate(float convergenceDistance)
        {
            return convergenceDistance > 1f ? 1f : convergenceDistance;
        }

        private void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > _maxDilation)
                _maxDilation = readDilation;
            if (readDilation < _minDilation)
                _minDilation = readDilation;
        }

        public void ResetThresholds()
        {
            _maxDilation = 0;
            _minDilation = 999;
        }
    }

    public class LipTrackingData
    {
        // Camera Data
        public (int x, int y) ImageSize;
        public byte[] ImageData;
        public bool SupportsImage;

        public float[] LatestShapes = new float[SRanipal_Lip_v2.WeightingCount];

        public void UpdateData(LipData_v2 lipData)
        {
            unsafe
            {
                for (int i = 0; i < SRanipal_Lip_v2.WeightingCount; i++)
                    LatestShapes[i] = lipData.prediction_data.blend_shape_weight[i];
            }
        }
    }

    public class UnifiedTrackingData
    {
        public static readonly IParameter[] AllParameters = EyeTrackingParams.ParameterList.Union(LipShapeMerger.AllLipParameters).ToArray();

        // Central update action for all parameters to subscribe to
        public static Action<EyeTrackingData /* Lip Data Blend Shape  */
            , LipTrackingData /* Lip Weightings */> OnUnifiedDataUpdated;

        public static EyeTrackingData LatestEyeData = new EyeTrackingData();
        public static LipTrackingData LatestLipData = new LipTrackingData();
    }
}