using System;
using System.Collections.Generic;
using System.Threading;
using MelonLoader;
using UnityEngine;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.SRParam;

namespace VRCEyeTracking
{
    public static class SRanipalTrack
    {
        private static readonly SRanipal_Eye_Framework Framework = new SRanipal_Eye_Framework();

        public static EyeData_v2 LatestEyeData;

        public static float CurrentDiameter;

        public static float MaxOpen;
        public static float MinOpen = 999;

        private static readonly Thread Updater = new Thread(Update);
        private static bool _trackingActive = true;

        public static void Start()
        {
            Framework.EnableEye = true;
            Framework.EnableEyeDataCallback = false;
            Framework.EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version1;
            Framework.StartFramework();
            SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
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

                    SRanipal_Eye_API.GetEyeData_v2(ref LatestEyeData);

                    if (LatestEyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                        .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                    {
                        CurrentDiameter = LatestEyeData.verbose_data.right.pupil_diameter_mm;
                        if (LatestEyeData.verbose_data.right.eye_openness >= 1f)
                            UpdateMinMaxDilation(LatestEyeData.verbose_data.right.pupil_diameter_mm);
                    }
                    else if (LatestEyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                        .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
                    {
                        CurrentDiameter = LatestEyeData.verbose_data.left.pupil_diameter_mm;
                        if (LatestEyeData.verbose_data.left.eye_openness >= 1f)
                            UpdateMinMaxDilation(LatestEyeData.verbose_data.left.pupil_diameter_mm);
                    }
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