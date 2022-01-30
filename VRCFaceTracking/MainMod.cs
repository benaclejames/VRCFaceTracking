using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using VRCFaceTracking;
using MelonLoader;
using UnityEngine;
using VRCFaceTracking.QuickMenu;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.6.0", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        // Mostly used for UI management, allows calling of main-thread methods directly from a tracking worker thread
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        public static readonly bool HasAdmin =
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public override void OnApplicationStart()
        {
            // Load all unmanaged DLLs as soon as we can
            DependencyManager.Init();
            UnifiedLibManager.Initialize();
            
            Hooking.SetupHooking();
            MelonCoroutines.Start(WaitForMenu());
            
            QuickModeMenu.LoadBundle();
        }

        public override void OnApplicationQuit() => UnifiedLibManager.Teardown();

        // Just waits for the user to open their quickmenu, easier than trying to work around the uninitialized cloned gameobjects
        private static IEnumerator WaitForMenu()
        {
            Transform qmCanvas = null;
            while (qmCanvas == null)
            {
                qmCanvas = GameObject.Find("UserInterface")?.transform?.FindChild("Canvas_QuickMenu(Clone)");
                yield return new WaitForSeconds(0.5f);
            }
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
    }
}
