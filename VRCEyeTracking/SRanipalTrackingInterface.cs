using System;
using System.Linq;
using System.Threading;
using MelonLoader;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.QuickMenu;

namespace VRCEyeTracking
{
    public static class SRanipalTrack
    {
        public static bool EyeEnabled, FaceEnabled;
        
        private static SRanipal_Eye_Framework _eyeFramework;
        private static SRanipal_Lip_Framework _lipFramework;

        public static EyeData_v2 LatestEyeData;
        public static LipData_v2 LatestLipData;

        public static float CurrentDiameter;

        public static float MaxOpen;
        public static float MinOpen = 999;

        private static readonly Thread Updater = new Thread(Update);
        
        private static bool IsRealError(this Error error) => error != Error.WORK && error != Error.UNDEFINED;

        public static void Initialize()
        {
            MelonLogger.Msg($"Initializing SRanipal...");
            
            var eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            var faceError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);

            HandleErrors(eyeError, faceError);
            UpdateConfigs(eyeError.IsRealError(), faceError.IsRealError());
            Updater.Start();
        }

        private static void HandleErrors(Error eyeError, Error faceError)
        {
            if (eyeError != Error.UNDEFINED && eyeError != Error.WORK)
                MelonLogger.Warning($"Eye Tracking will be unavailable for this session. ({eyeError})");
            else if (eyeError == Error.WORK)
                MelonLogger.Msg("SRanipal Eye Initialized!");
            
            if (faceError != Error.UNDEFINED && faceError != Error.WORK)
                MelonLogger.Warning($"Lip Tracking will be unavailable for this session. ({faceError})");
            else if (faceError == Error.WORK)
                MelonLogger.Msg("SRanipal Lip Initialized!");
        }

        private static void UpdateConfigs(bool eyeError = true, bool faceError = true)
        {
            // Init Eye Framework
            _eyeFramework = new SRanipal_Eye_Framework();
            _eyeFramework.EnableEye = !eyeError;
            _eyeFramework.EnableEyeDataCallback = false;
            _eyeFramework.EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version2;
            if (!eyeError) _eyeFramework.StartFramework();

            if (QuickModeMenu.HasInitMenu && QuickModeMenu.EyeTab != null && !eyeError)
                QuickModeMenu.EyeTab.TabEnabled = true;

            EyeEnabled = !eyeError;
            
            
            
            // Init Lip Framework
            _lipFramework = new SRanipal_Lip_Framework();
            _lipFramework.EnableLip = !faceError;
            _lipFramework.EnableLipVersion = SRanipal_Lip_Framework.SupportedLipVersion.version2;
            if (!faceError) _lipFramework.StartFramework();

            FaceEnabled = !faceError;
        }

        public static void Stop()
        {
            EyeEnabled = false;
            FaceEnabled = false;
            Updater.Abort();
            
            _eyeFramework?.StopFramework();
            _lipFramework?.StopFramework();
        }

        private static void Update()
        {
            while (EyeEnabled || FaceEnabled)
            {
                try
                {
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    if (EyeEnabled) UpdateEye();
                    if (FaceEnabled) UpdateMouth();
                }
                catch (Exception e)
                {
                    if (e.InnerException.GetType() != typeof(ThreadAbortException))
                        MelonLogger.Error("Threading error occured in SRanipalTrack.Update: "+e+": "+e.InnerException);
                }

                Thread.Sleep(5);
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

        private static void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > MaxOpen)
                MaxOpen = readDilation;
            if (readDilation < MinOpen)
                MinOpen = readDilation;
        }
        
        #endregion

        #region MouthUpdate

        private static void UpdateMouth()
        {
            SRanipal_Lip_API.GetLipData_v2(ref LatestLipData);
        }

        #endregion
    }
}