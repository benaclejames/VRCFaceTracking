using System;
using System.Collections;
using System.Collections.Generic;
using VRCFaceTracking;
using MelonLoader;
using UnityEngine;
using VRCFaceTracking.QuickMenu;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.5.0", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        // Mostly used for UI management, allows calling of main-thread methods directly from a tracking worker thread
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        public override void OnApplicationStart()
        {
            // Load all unmanaged DLLs as soon as we can
            DependencyManager.Init();

            QuickModeMenu.LoadBundle();
            MelonCoroutines.Start(CheckUiManager());
        }
        
        public override void OnApplicationQuit() => UnifiedLibManager.Teardown();

        private static void UiManagerInit()
        {
            UnifiedLibManager.Initialize();
            Hooking.SetupHooking();
            MelonCoroutines.Start(WaitForMenu());
        }

        // Just waits for the user to open their quickmenu, easier than trying to work around the uninitialized cloned gameobjects
        private static IEnumerator WaitForMenu()
        {
            var qmCanvas = GameObject.Find("UserInterface").transform.FindChild("Canvas_QuickMenu(Clone)");
            yield return new WaitUntil((Func<bool>) (() => qmCanvas.gameObject.active));
            QuickModeMenu.CheckIfShouldInit();
        }
        
        public override void OnSceneWasLoaded(int level, string levelName) =>
            UnifiedTrackingData.LatestEyeData.ResetThresholds();

        public override void OnUpdate()
        {
            if (!UnifiedLibManager.ShouldThread) 
                UnifiedLibManager.Update();

            UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                
            if (QuickModeMenu.MainMenu != null && QuickModeMenu.IsMenuShown) 
                QuickModeMenu.MainMenu.UpdateParams();
            
            if (MainThreadExecutionQueue.Count <= 0) return;
            
            MainThreadExecutionQueue[0].Invoke();
            MainThreadExecutionQueue.RemoveAt(0);
        }

        private IEnumerator CheckUiManager()
        {
            yield return new WaitUntil((Func<bool>) (() =>
                VRCPlayer.field_Internal_Static_VRCPlayer_0 != null));

            UiManagerInit();
        }
    }
}
