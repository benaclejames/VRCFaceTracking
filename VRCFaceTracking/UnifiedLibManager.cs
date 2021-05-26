using System;
using System.Threading;
using MelonLoader;
using UnityEngine.SceneManagement;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Pimax;
using VRCFaceTracking.QuickMenu;
using VRCFaceTracking.SRanipal;

namespace VRCFaceTracking
{
    public static class UnifiedLibManager
    {
        private static bool _isUsingSRanipal;
        public static bool EyeEnabled, LipEnabled;
        private static bool _isInitializing;
        
        public static readonly Thread Initializer = new Thread(() => Initialize());
        private static bool ShouldUsePimax(Error eyeError) => eyeError == Error.RUNTIME_NOT_FOUND;
        
        
        public static void Initialize(bool eye = true, bool lip = true)
        {
            if (_isInitializing) return;
            _isInitializing = true;

            var (eyeError, lipError) = SRanipalTrackingInterface.Initialize(eye, lip);
            _isUsingSRanipal = !ShouldUsePimax(eyeError);
            
            if (_isUsingSRanipal)
            {
                HandleSrErrors(eyeError, lipError);
                if (!SRanipalTrackingInterface.SRanipalWorker.IsAlive) 
                    SRanipalTrackingInterface.SRanipalWorker.Start();
            }
            else
            {
                if (eye)
                    EyeEnabled = PimaxTrackingInterface.Initialize();
                
                if (EyeEnabled && !PimaxTrackingInterface.PimaxWorker.IsAlive) 
                    PimaxTrackingInterface.PimaxWorker.Start();
            }
            
            
            if (EyeEnabled) MainMod.AppendEyeParams();
            if (LipEnabled) MainMod.AppendLipParams();

            if (SceneManager.GetActiveScene().buildIndex == -1 && QuickModeMenu.MainMenu != null)
                MainMod.MainThreadExecutionQueue.Add(() => QuickModeMenu.MainMenu.UpdateEnabledTabs(EyeEnabled, LipEnabled));

            _isInitializing = false;
        }

        public static void Teardown()
        {
            if (_isUsingSRanipal) SRanipalTrackingInterface.Stop();
        }

        private static void HandleSrErrors(Error eyeError, Error lipError)
        {
            if (eyeError.IsRealError())
                // Msg instead of Warning under the assumption most people will be using only lip tracking
                MelonLogger.Msg($"Eye Tracking will be unavailable for this session. ({eyeError})");
            else if (eyeError == Error.WORK)
            {
                EyeEnabled = true;
                MelonLogger.Msg("SRanipal Eye Initialized!");
            }

            if (lipError.IsRealError())
                MelonLogger.Warning($"Lip Tracking will be unavailable for this session. ({lipError})");
            else if (lipError == Error.FOXIP_SO)
                while (lipError == Error.FOXIP_SO)
                    lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            if (lipError != Error.WORK) return;
            
            LipEnabled = true;
            MelonLogger.Msg("SRanipal Lip Initialized!");
        }
    }
}