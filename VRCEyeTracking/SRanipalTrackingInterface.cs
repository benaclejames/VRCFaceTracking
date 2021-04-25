using System;
using System.Collections.Generic;
using System.Threading;
using MelonLoader;
using UnityEngine.SceneManagement;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.QuickMenu;
using VRCEyeTracking.SRParam.LipMerging;

namespace VRCEyeTracking
{
    public static class SRanipalTrack
    {
        public static bool EyeEnabled, FaceEnabled;
        

        public static EyeData_v2 LatestEyeData;
        public static Dictionary<LipShape_v2, float> LatestLipData;

        public static float CurrentDiameter;

        public static float MaxDilation;
        public static float MinDilation = 999;

        public static readonly Thread Initializer = new Thread(() => Initialize());
        private static readonly Thread SRanipalWorker = new Thread(() => Update(CancellationToken.Token));
        
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool IsRealError(this Error error) => error != Error.WORK && error != Error.UNDEFINED && error != (Error) 1051;

        public static void Initialize(bool eye = true, bool lip = true)
        {
            MelonLogger.Msg($"Initializing SRanipal...");

            Error eyeError = Error.UNDEFINED, faceError = Error.UNDEFINED;

            if (eye)
            {
                if (EyeEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized eye module...");
                    SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
                }

                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            }

            if (lip)
            {
                if (FaceEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized lip module...");
                    SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
                }
                
                faceError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            }

            HandleErrors(eyeError, faceError);
            
            if (SceneManager.GetActiveScene().buildIndex == -1)
                MainMod.MainThreadExecutionQueue.Add(QuickModeMenu.CheckIfShouldInit);
            
            if (!SRanipalWorker.IsAlive) SRanipalWorker.Start();
        }

        private static void HandleErrors(Error eyeError, Error faceError)
        {
            if (eyeError.IsRealError())
                // Msg instead of Warning under the assumption most people will be using only lip tracking
                MelonLogger.Msg($"Eye Tracking will be unavailable for this session. ({eyeError})");
            else if (eyeError == Error.WORK)
            {
                MainMod.AppendEyeParams();
                EyeEnabled = true;
                MelonLogger.Msg("SRanipal Eye Initialized!");
            }

            if (faceError.IsRealError())
                MelonLogger.Warning($"Lip Tracking will be unavailable for this session. ({faceError})");
            else if (faceError == (Error) 1051)
                while (faceError == (Error) 1051)
                    faceError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            if (faceError == Error.WORK)
            {
                MainMod.AppendLipParams();
                FaceEnabled = true;
                MelonLogger.Msg("SRanipal Lip Initialized!");
            }
        }

        public static void Stop()
        {
            CancellationToken.Cancel();
            
            if (EyeEnabled) SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
            if (FaceEnabled) SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
            
            CancellationToken.Dispose();
        }

        private static void Update(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (EyeEnabled) UpdateEye();
                    if (FaceEnabled) UpdateMouth();
                }
                catch (Exception e)
                {
                    if (e.InnerException.GetType() != typeof(ThreadAbortException))
                        MelonLogger.Error("Threading error occured in SRanipalTrack.Update: "+e+": "+e.InnerException);
                }
                Thread.Sleep(10);
            }
        }
        
        #region EyeUpdate

        private static void UpdateEye()
        {
            SRanipal_Eye_API.GetEyeData_v2(ref LatestEyeData);

            if (LatestEyeData.verbose_data.right.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                CurrentDiameter = LatestEyeData.verbose_data.right.pupil_diameter_mm;
                UpdateMinMaxDilation(LatestEyeData.verbose_data.right.pupil_diameter_mm);
            }
            else if (LatestEyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                CurrentDiameter = LatestEyeData.verbose_data.left.pupil_diameter_mm;
                UpdateMinMaxDilation(LatestEyeData.verbose_data.left.pupil_diameter_mm);
            }
        }

        private static void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > MaxDilation)
                MaxDilation = readDilation;
            if (readDilation < MinDilation)
                MinDilation = readDilation;
        }
        
        #endregion

        #region MouthUpdate

        private static void UpdateMouth()
        {
            SRanipal_Lip_v2.GetLipWeightings(out LatestLipData);
        }

        #endregion

        public static void ResetTrackingThresholds()
        {
            MinDilation = 999;
            MaxDilation = 0;
            
            LipShapeMerger.ResetLipShapeMinMaxThresholds();
        }
    }
}