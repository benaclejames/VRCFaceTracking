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

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.4.1", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        public static void ResetParams() => _currentlyTrackedParams = FindParams(ParamLib.ParamLib.GetLocalParams().Select(p => p.name).Distinct());
        
        public static void ZeroParams() => _currentlyTrackedParams.Clear();

        public override void OnApplicationStart()
        {
            DependencyManager.Init();

            _assemblyCSharp = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
            _shouldCheckUiManager = typeof(MelonMod).GetMethod("VRChat_OnUiManagerInit") == null;
        }

        public override void OnApplicationQuit() => UnifiedLibManager.Teardown();

        private static List<IParameter> _currentlyTrackedParams = new List<IParameter>();

        private Assembly _assemblyCSharp;
        private Type _uiManager;
        private MethodInfo _uiManagerInstance;
        private bool _shouldCheckUiManager;

        public static Action<EyeTrackingData, float[], Dictionary<LipShape_v2, float>> OnUnifiedParamsUpdated = (eye, lip, floats) => { };

        private static List<IParameter> FindParams(IEnumerable<string> searchParams)
        {
            var eyeParams = EyeTrackingParams.ParameterList.Where(p => p.GetName().Any(searchParams.Contains));
            
            var optimizedLipParams = LipShapeMerger.GetOptimizedLipParameters().Where(p => p.GetName().Any(searchParams.Contains));
            
            var unoptimizedLipParams = LipShapeMerger.GetAllLipShapes()
                .Where(shape => searchParams.Contains(shape.ToString()))
                .Select(unoptimizedShape => new LipParameter(unoptimizedShape.ToString(), (eye, lip) =>
                {
                    if (eye.TryGetValue(unoptimizedShape, out var retValue)) return retValue;
                    return null;
                }, true))
                .Cast<IParameter>()
                .ToList();

            return eyeParams.Union(optimizedLipParams).Union(unoptimizedLipParams).ToList();
        }

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

        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();
        
        public override void OnUpdate()
        {
            if (!UnifiedLibManager.ShouldThread) 
                UnifiedLibManager.Update();
            
            if (_shouldCheckUiManager) CheckUiManager();
            
            OnUnifiedParamsUpdated.Invoke(UnifiedTrackingData.LatestEyeData, UnifiedTrackingData.LatestLipData.prediction_data.blend_shape_weight, UnifiedTrackingData.LatestLipShapes);
                
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
