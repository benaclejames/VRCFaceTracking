using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRCFaceTracking;
using MelonLoader;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.LipMerging;
using VRCFaceTracking.QuickMenu;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.4.1", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        // Detect when UIManager has finished initializing
        private Assembly _assemblyCSharp;  
        private Type _uiManager;
        private MethodInfo _uiManagerInstance;
        private bool _shouldCheckUiManager;
        
        // Mostly used for UI management, allows calling of main-thread methods directly from a tracking worker thread
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        public override void OnApplicationStart()
        {
            // Load all unmanaged DLLs as soon as we can
            DependencyManager.Init();

            // Prepare to watch for UIManager initialization
            _assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
            _shouldCheckUiManager = typeof(MelonMod).GetMethod("VRChat_OnUiManagerInit") == null;
        }
        
        public override void OnApplicationQuit() => UnifiedLibManager.Teardown();

        private static void UiManagerInit()
        {
            UnifiedLibManager.Initialize();
            Hooking.SetupHooking();
        }
        
        public override void OnSceneWasLoaded(int level, string levelName)
        {
            if (level == -1)
                QuickModeMenu.CheckIfShouldInit();

            UnifiedTrackingData.LatestEyeData.ResetThresholds();
        }

        public override void OnUpdate()
        {
            if (!UnifiedLibManager.ShouldThread) 
                UnifiedLibManager.Update();
            
            if (_shouldCheckUiManager) CheckUiManager();
            
            UnifiedTrackingData.OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                
            if (QuickModeMenu.MainMenu != null && QuickModeMenu.IsMenuShown) 
                QuickModeMenu.MainMenu.UpdateParams();
            
            if (MainThreadExecutionQueue.Count <= 0) return;
            
            MainThreadExecutionQueue[0].Invoke();
            MainThreadExecutionQueue.RemoveAt(0);
        }

        private void CheckUiManager()
        {
            if (_assemblyCSharp == null) return;
            
            if (_uiManager == null) _uiManager = _assemblyCSharp.GetType("VRCUiManager");
            if (_uiManager == null) {
                _shouldCheckUiManager = false;
                return;
            }
            
            if (_uiManagerInstance == null)
                _uiManagerInstance = _uiManager.GetMethods().First(x => x.ReturnType == _uiManager);
            if (_uiManagerInstance == null)
            {
                _shouldCheckUiManager = false;
                return;
            }

            if (_uiManagerInstance.Invoke(null, Array.Empty<object>()) == null)
                return;

            _shouldCheckUiManager = false;
            UiManagerInit();
        }
    }
}
