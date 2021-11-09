using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using ParamLib;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.LipMerging;
using VRCFaceTracking.Pimax;

namespace VRCFaceTracking
{
    // Represents a single eye, can also be used as a combined eye
    public struct Eye
    {
        public Vector2? Look;
        public float Openness;
        public float Widen, Squeeze;

        
        public void Update(SingleEyeData eyeData, SingleEyeExpression? expression = null)
        {
            Look = null;
            Openness = 0;
            Widen = 0;
            Squeeze = 0;

            if (eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY))
                Look = Vector3.Scale(
                    eyeData.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));

            Openness = eyeData.eye_openness;
            
            if (expression == null) return; // This is null when we use this as a combined eye, so don't try read data from it
            
            Widen = expression.Value.eye_wide;
            Squeeze = expression.Value.eye_squeeze;
        }

        public void Update(EyeExpressionState eyeState)
        {
            Look = new Vector2(eyeState.PupilCenterX, eyeState.PupilCenterY);
            Openness = eyeState.Openness;
            Widen = 0;
            Squeeze = 0;
        }
    }
    
    public struct EyeTrackingData
    {
        public Eye Left, Right, Combined;
        
        // SRanipal Exclusive
        public float EyesDilation;
        private float _maxDilation, _minDilation;


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

            if (dilation != 0)
                EyesDilation = dilation / _minDilation / (_maxDilation - _minDilation);
        }

        public void UpdateData(Ai1EyeData eyeData)
        {
            Left.Update(eyeData.Left);
            Right.Update(eyeData.Right);
            Combined.Update(eyeData.Recommended);
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

    public struct UnifiedTrackingData
    {
        // Central update action for all parameters to subscribe to
        public static Action<EyeTrackingData, float[] /* Lip Data Blend Shape  */, Dictionary<LipShape_v2, float> /* Lip Weightings */> OnUnifiedParamsUpdated = (eye, lip, floats) => { };
        
        // List of parameter objects the current avatar is using (This is not where the parameters are stored, but rather a way to know which of the existing stored parameters are being used)
        private static List<IParameter> _currentlyUsedParams = new List<IParameter>();
        
        // Copy of latest updated unified eye data
        public static EyeTrackingData LatestEyeData;
        
        // SRanipal Exclusives
        public static LipData_v2 LatestLipData;
        public static Dictionary<LipShape_v2, float> LatestLipShapes;

        // Resets the currently used params list and regenerates it with the latest found parameters
        public static void RefreshParameterList()
        {
            foreach (var baseParam in FloatBaseParam.PrioritisedParams)
                MelonCoroutines.Stop(baseParam);
            foreach (var current in _currentlyUsedParams)
            {
                current.ZeroParam();
                current.ResetParam();
            }

            _currentlyUsedParams = FindParams(ParamLib.ParamLib.GetLocalParams().Select(p => p.name).Distinct());
        }


        // Returns a list of all parameters given by name in the searchParams parameter
        public static List<IParameter> FindParams(IEnumerable<string> searchParams)
        {
            var eyeParams = EyeTrackingParams.ParameterList.Where(p => p.GetName().Any(searchParams.Contains));
            
            var optimizedLipParams = LipShapeMerger.AllLipParameters.Where(p => p.GetName().Any(searchParams.Contains));

            return eyeParams.Union(optimizedLipParams).ToList();
        }
    }
}