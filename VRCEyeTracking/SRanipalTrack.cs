using System;
using System.Collections.Generic;
using System.Threading;
using MelonLoader;
using UnityEngine;
using ViveSR.anipal;
using ViveSR.anipal.Eye;

namespace VRCEyeTracking
{
    public static class SRanipalTrack
    {
        private static readonly SRanipal_Eye_Framework Framework = new SRanipal_Eye_Framework();

        private static EyeData_v2 _latestEyeData;

        private static float _currentDiameter;

        public static float MaxOpen;
        public static float MinOpen = 999;

        public static readonly Dictionary<string, float> SRanipalData = new Dictionary<string, float>
        {
            {"EyesX", 0},
            {"EyesY", 0},
            {"LeftEyeLid", 0},
            {"RightEyeLid", 0},
            {"EyesWiden", 0},
            {"EyesDilation", 0},
            {"EyesSqueeze", 0},
            {"LeftEyeX", 0},
            {"LeftEyeY", 0},
            {"RightEyeX", 0},
            {"RightEyeY", 0},
            {"RightEyeWiden", 0},
            {"LeftEyeWiden", 0},
            {"LeftEyeSqueeze", 0},
            {"RightEyeSqueeze", 0}
        };

        private static readonly Thread Updater = new Thread(Update);
        private static bool _trackingActive = true;
    
        public static void Start()
        {
            Framework.EnableEye = true;
            Framework.EnableEyeDataCallback = false;
            Framework.EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version1;
            Framework.StartFramework();
            SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            Updater.Start();
        }

        public static void Stop()
        {
            _trackingActive = false;
            Updater.Abort();
            Framework.StopFramework();
        }

        private static void Update()
        {
            while (_trackingActive)
            {
                try
                {
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    SRanipal_Eye_API.GetEyeData_v2(ref _latestEyeData);
                
                    var combinedEyeReverse = Vector3.Scale(
                        _latestEyeData.verbose_data.combined.eye_data.gaze_direction_normalized,
                        new Vector3(-1, 1, 1));
                
                    var leftEyeReverse = Vector3.Scale(_latestEyeData.verbose_data.left.gaze_direction_normalized,
                        new Vector3(-1, 1, 1));
                
                    var rightEyeReverse = Vector3.Scale(_latestEyeData.verbose_data.right.gaze_direction_normalized,
                        new Vector3(-1, 1, 1));

                    if (combinedEyeReverse != new Vector3(1.0f, -1.0f, -1.0f))
                    {
                        SRanipalData["EyesX"] = combinedEyeReverse.x;
                        SRanipalData["EyesY"] = combinedEyeReverse.y;

                        SRanipalData["LeftEyeX"] = leftEyeReverse.x;
                        SRanipalData["LeftEyeY"] = leftEyeReverse.y;

                        SRanipalData["RightEyeX"] = rightEyeReverse.x;
                        SRanipalData["RightEyeY"] = rightEyeReverse.y;
                    }

                    SRanipalData["LeftEyeLid"] = _latestEyeData.verbose_data.left.eye_openness;
                    SRanipalData["RightEyeLid"] = _latestEyeData.verbose_data.right.eye_openness;

                    if (_latestEyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                        .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                    {
                        _currentDiameter = _latestEyeData.verbose_data.right.pupil_diameter_mm;
                        if (_latestEyeData.verbose_data.right.eye_openness >= 1f)
                            UpdateMinMaxDilation(_latestEyeData.verbose_data.right.pupil_diameter_mm);
                    }
                    else if (_latestEyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                        .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                    {
                        _currentDiameter = _latestEyeData.verbose_data.left.pupil_diameter_mm;
                        if (_latestEyeData.verbose_data.left.eye_openness >= 1f)
                            UpdateMinMaxDilation(_latestEyeData.verbose_data.left.pupil_diameter_mm);
                    }

                    var normalizedFloat = _currentDiameter / MinOpen / (MaxOpen - MinOpen);
                    SRanipalData["EyesDilation"] = Mathf.Clamp(normalizedFloat, 0, 1);

                    SRanipalData["EyesWiden"] = _latestEyeData.expression_data.left.eye_wide >
                                                _latestEyeData.expression_data.right.eye_wide
                        ? _latestEyeData.expression_data.left.eye_wide
                        : _latestEyeData.expression_data.right.eye_wide;

                    SRanipalData["LeftEyeWiden"] = _latestEyeData.expression_data.left.eye_wide;
                    SRanipalData["RightEyeWiden"] = _latestEyeData.expression_data.right.eye_wide;

                    SRanipalData["LeftEyeSqueeze"] = _latestEyeData.expression_data.left.eye_squeeze;
                    SRanipalData["RightEyeSqueeze"] = _latestEyeData.expression_data.right.eye_squeeze;
                
                    SRanipalData["EyesSqueeze"] = _latestEyeData.expression_data.left.eye_squeeze >
                                                  _latestEyeData.expression_data.right.eye_squeeze
                        ? _latestEyeData.expression_data.left.eye_squeeze
                        : _latestEyeData.expression_data.right.eye_squeeze;
                }
                catch (Exception e)
                {
                    if (!(e.InnerException is ThreadAbortException))
                        MelonLogger.Error("Threading error occured in SRanipalTrack.Update: "+e);
                }

                Thread.Sleep(5);
            }
        }

        private static void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > MaxOpen)
                MaxOpen = readDilation;
            if (readDilation < MinOpen)
                MinOpen = readDilation;
        }
    }
}