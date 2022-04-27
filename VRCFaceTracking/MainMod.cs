using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using VRCFaceTracking;
using MelonLoader;
using UnityEngine;
using VRC.UI.Core;
using VRCFaceTracking.QuickMenu;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.6.2", "benaclejames & Fenrix",
    "https://github.com/FenrixTheFox/VRCFaceTracking")]
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
        
        private static IEnumerator WaitForMenu()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null)
                yield return null;
            
            while (UIManager.field_Private_Static_UIManager_0 == null)
                yield return null;
            
            while (GameObject.Find("UserInterface").GetComponentInChildren<VRC.UI.Elements.QuickMenu>(true) == null)
                yield return null;
            
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
