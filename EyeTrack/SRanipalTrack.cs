using System;
using System.Collections.Generic;
using System.Threading;
using MelonLoader;
using UnityEngine;
using ViveSR.anipal;
using ViveSR.anipal.Eye;

public class SRanipalTrack
{
    private static readonly SRanipal_Eye_Framework _framework = new SRanipal_Eye_Framework();

    private static EyeData_v2 _latestEyeData;

    private static float CurrentDiameter;

    public static float MaxOpen;
    public static float MinOpen = 999;

    public static Dictionary<string, float> SRanipalData = new Dictionary<string, float>
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

    private static Thread Updater = new Thread(Update);
    
    public static void Start()
    {
        _framework.EnableEye = true;
        _framework.EnableEyeDataCallback = false;
        _framework.EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version1;
        _framework.StartFramework();
        SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
        Updater.Start();
    }

    public static void Stop()
    {
        Updater.Abort();
        _framework.StopFramework();
    }

    private static void Update()
    {
        for (;;)
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
                var CombinedEyeReverse = Vector3.Scale(
                    _latestEyeData.verbose_data.combined.eye_data.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
                var LeftEyeReverse = Vector3.Scale(_latestEyeData.verbose_data.left.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));
                var RightEyeReverse = Vector3.Scale(_latestEyeData.verbose_data.right.gaze_direction_normalized,
                    new Vector3(-1, 1, 1));

                if (CombinedEyeReverse != new Vector3(1.0f, -1.0f, -1.0f))
                {
                    SRanipalData["EyesX"] = CombinedEyeReverse.x;
                    SRanipalData["EyesY"] = CombinedEyeReverse.y;

                    SRanipalData["LeftEyeX"] = LeftEyeReverse.x;
                    SRanipalData["LeftEyeY"] = LeftEyeReverse.y;

                    SRanipalData["RightEyeX"] = RightEyeReverse.x;
                    SRanipalData["RightEyeY"] = RightEyeReverse.y;
                }

                SRanipalData["LeftEyeLid"] = _latestEyeData.verbose_data.left.eye_openness;
                SRanipalData["RightEyeLid"] = _latestEyeData.verbose_data.right.eye_openness;

                if (_latestEyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                    .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                {
                    CurrentDiameter = _latestEyeData.verbose_data.right.pupil_diameter_mm;
                    if (_latestEyeData.verbose_data.right.eye_openness >= 1f)
                        UpdateMinMaxDilation(_latestEyeData.verbose_data.right.pupil_diameter_mm);
                }
                else if (_latestEyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                    .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                {
                    CurrentDiameter = _latestEyeData.verbose_data.left.pupil_diameter_mm;
                    if (_latestEyeData.verbose_data.left.eye_openness >= 1f)
                        UpdateMinMaxDilation(_latestEyeData.verbose_data.left.pupil_diameter_mm);
                }

                var normalizedFloat = CurrentDiameter / MinOpen / (MaxOpen - MinOpen);
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
                    MelonLogger.Error("Threading error occured in SRanipalTrack.Update: "+e.Message);
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