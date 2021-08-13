using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRCFaceTracking;
using MelonLoader;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.LipMerging;
using VRCFaceTracking.QuickMenu;
using VRCFaceTracking.SRanipal;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.1.5", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        public static void ResetParams() => SRanipalTrackParams.ForEach(param => param.ResetParam());
        public static void ZeroParams() => SRanipalTrackParams.ForEach(param => param.ZeroParam());
        public override void OnApplicationStart()
        {
            DependencyManager.Init();

            _assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
            _shouldCheckUiManager = typeof(MelonMod).GetMethod("VRChat_OnUiManagerInit") == null;
        }

        public override void OnApplicationQuit() => UnifiedLibManager.Teardown();

        private static readonly List<IParameter> SRanipalTrackParams = new List<IParameter>();

        private Assembly _assemblyCSharp;
        private Type _uiManager;
        private MethodInfo _uiManagerInstance;
        private bool _shouldCheckUiManager;

        public static Action<EyeTrackingData, float[], Dictionary<LipShape_v2, float>> OnSRanipalParamsUpdated = (eye, lip, floats) => { };

        private static void AppendEyeParams() => SRanipalTrackParams.AddRange(EyeTrackingParams.ParameterList);
        
        private static void AppendLipParams()
        {
            // Add optimized shapes
            SRanipalTrackParams.AddRange(LipShapeMerger.GetOptimizedLipParameters());
            
            // Add unoptimized shapes in case someone wants to use em
            foreach (var unoptimizedShape in LipShapeMerger.GetAllLipShapes())
                SRanipalTrackParams.Add(new LipParameter(unoptimizedShape.ToString(), 
                    (eye, lip) =>
                    {
                        if (eye.TryGetValue(unoptimizedShape, out var retValue)) return retValue;
                        return null;
                    }));
        }

        private static void UiManagerInit()
        {
            AppendEyeParams();
            AppendLipParams();

            UnifiedLibManager.Initialize();
            Hooking.SetupHooking();
        }
        
        public override void OnSceneWasLoaded(int level, string levelName)
        {
            if (level == -1)
                QuickModeMenu.CheckIfShouldInit();
            
            SRanipalTrackingInterface.ResetTrackingThresholds();
        }

        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();
        
        public override void OnUpdate()
        {
            if (!UnifiedLibManager.ShouldThread) 
                UnifiedLibManager.Update();
            
            if (_shouldCheckUiManager) CheckUiManager();
            
            OnSRanipalParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                
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

            if (_uiManagerInstance.Invoke(null, new object[0]) == null)
                return;

            _shouldCheckUiManager = false;
            UiManagerInit();
        }
    }
}
