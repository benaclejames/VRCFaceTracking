using System;
using System.Collections.Generic;
using System.Threading;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.QuickMenu;
using VRCFaceTracking.SRParam.LipMerging;

namespace VRCFaceTracking
{
    public static class SRanipalTrack
    {
        public static bool EyeEnabled, LipEnabled;

        public static EyeData_v2 LatestEyeData;
        public static LipData_v2 LatestLipData;
        public static Dictionary<LipShape_v2, float> LatestLipShapes;

        public static float CurrentDiameter;

        public static float MaxDilation;
        public static float MinDilation = 999;

        public static readonly Thread Initializer = new Thread(() => Initialize());
        private static readonly Thread SRanipalWorker = new Thread(() => Update(CancellationToken.Token));
        
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        private static bool _isInitializing;
        
        private static bool IsRealError(this Error error) => error != Error.WORK && error != Error.UNDEFINED && error != (Error) 1051;

        public static void Initialize(bool eye = true, bool lip = true)
        {
            if (_isInitializing) return;
            _isInitializing = true;
            
            MelonLogger.Msg($"Initializing SRanipal...");

            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye)
            {
                if (EyeEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized eye module...");
                    SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
                    Thread.Sleep(5000); // Give SRanipal a chance to finish restarting
                }

                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            }

            if (lip)
            {
                if (LipEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized lip module...");
                    SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
                    Thread.Sleep(5000); // Give SRanipal a chance to finish restarting
                }
                
                lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            }

            HandleErrors(eyeError, lipError);
            
            if (SceneManager.GetActiveScene().buildIndex == -1 && QuickModeMenu.MainMenu != null)
                MainMod.MainThreadExecutionQueue.Add(() => QuickModeMenu.MainMenu.UpdateEnabledTabs(EyeEnabled, LipEnabled));
            
            if (!SRanipalWorker.IsAlive) SRanipalWorker.Start();
            _isInitializing = false;
        }

        private static void HandleErrors(Error eyeError, Error lipError)
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

            if (lipError.IsRealError())
                MelonLogger.Warning($"Lip Tracking will be unavailable for this session. ({lipError})");
            else if (lipError == (Error) 1051)
                while (lipError == (Error) 1051)
                    lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            if (lipError != Error.WORK) return;
            
            MainMod.AppendLipParams();
            LipEnabled = true;
            MelonLogger.Msg("SRanipal Lip Initialized!");
        }

        public static void Stop()
        {
            CancellationToken.Cancel();
            
            if (EyeEnabled) SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
            if (LipEnabled) SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
            
            CancellationToken.Dispose();
        }

        private static void Update(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (EyeEnabled) UpdateEye();
                    if (LipEnabled) UpdateMouth();
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
            SRanipal_Lip_API.GetLipData_v2(ref LatestLipData);
            SRanipal_Lip_v2.GetLipWeightings(out LatestLipShapes);
        }

        public static Texture2D UpdateLipTexture()
        {
            var lipTexture = new Texture2D(800, 400, TextureFormat.Alpha8, false);
            return SRanipal_Lip_v2.GetLipImage(ref lipTexture) ? lipTexture : null;
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