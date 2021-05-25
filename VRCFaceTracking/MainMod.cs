using System;
using System.Collections.Generic;
using VRCFaceTracking;
using MelonLoader;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.QuickMenu;
using VRCFaceTracking.SRParam;
using VRCFaceTracking.SRParam.LipMerging;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.1.0", "benaclejames",
    "https://github.com/benaclejames/VRCFaceTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCFaceTracking
{
    public class MainMod : MelonMod
    {
        public static void ResetParams() => SRanipalTrackParams.ForEach(param => param.ResetParam());
        public static void ZeroParams() => SRanipalTrackParams.ForEach(param => param.ZeroParam());
        public static void AppendEyeParams() => SRanipalTrackParams.AddRange(EyeTrackingParams.ParameterList);
        public override void OnApplicationStart() => DependencyManager.Init();
        public override void OnApplicationQuit() => SRanipalTrack.Stop();

        private static readonly List<ISRanipalParam> SRanipalTrackParams = new List<ISRanipalParam>();

        public static Action<EyeData_v2?, float[], Dictionary<LipShape_v2, float>> OnSRanipalParamsUpdated = (eye, lip, floats) => { };
        
        public static void AppendLipParams()
        {
            // Add optimized shapes
            SRanipalTrackParams.AddRange(LipShapeMerger.GetOptimizedLipParameters());
            
            // Add viseme mirroring shapes
            SRanipalTrackParams.AddRange(LipShapeMerger.VisemeShapes);
            
            // Add unoptimized shapes in case someone wants to use em
            foreach (var unoptimizedShape in LipShapeMerger.GetAllLipShapes())
                SRanipalTrackParams.Add(new SRanipalLipParameter(unoptimizedShape.ToString(), 
                    (eye, lip) =>
                    {
                        if (eye.TryGetValue(unoptimizedShape, out var retValue)) return retValue;
                        return null;
                    }));
        }

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Initializer.Start();
            Hooking.SetupHooking();
        }
        
        public override void OnSceneWasLoaded(int level, string levelName)
        {
            if (level == -1)
                QuickModeMenu.CheckIfShouldInit();
            
            SRanipalTrack.ResetTrackingThresholds();
        }

        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();
        
        public override void OnUpdate()
        {
            OnSRanipalParamsUpdated.Invoke(SRanipalTrack.LatestEyeData, SRanipalTrack.LatestLipData.prediction_data.blend_shape_weight, SRanipalTrack.LatestLipShapes);
                
            if (QuickModeMenu.MainMenu != null && QuickModeMenu.IsMenuShown) 
                QuickModeMenu.MainMenu.UpdateParams(SRanipalTrack.LatestEyeData, SRanipalTrack.UpdateLipTexture());
            
            if (MainThreadExecutionQueue.Count <= 0) return;
            
            MainThreadExecutionQueue[0].Invoke();
            MainThreadExecutionQueue.RemoveAt(0);
        }
    }
}
