using System;
using System.Collections;
using System.Collections.Generic;
using VRCFaceTracking;
using MelonLoader;
using UnityEngine;
using VRCFaceTracking.QuickMenu;
using VRCFaceTracking.SRParam;
using VRCFaceTracking.SRParam.LipMerging;

[assembly: MelonInfo(typeof(MainMod), "VRCFaceTracking", "2.0.1", "benaclejames",
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
        
        public static void AppendLipParams()
        {
            // Add optimized shapes
            SRanipalTrackParams.AddRange(LipShapeMerger.GetOptimizedLipParameters());
            
            // Add unoptimized shapes in case someone wants to use em
            foreach (var unoptimizedShape in LipShapeMerger.GetUnoptimizedLipShapes())
                SRanipalTrackParams.Add(new SRanipalLipParameter(v2 =>
                    {
                        if (v2.TryGetValue(unoptimizedShape, out var retValue)) return retValue;
                        return null;
                    }, 
                    unoptimizedShape.ToString(), true));
        }

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Initializer.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdateParams());
        }
        
        public override void OnSceneWasLoaded(int level, string levelName)
        {
            if (level == -1)
                QuickModeMenu.CheckIfShouldInit();
            
            SRanipalTrack.ResetTrackingThresholds();
        }
        

        // Refreshing in main thread to avoid threading errors
        private static IEnumerator UpdateParams()
        {
            for (;;)
            {
                foreach (var sRanipalParam in SRanipalTrackParams.ToArray())
                    sRanipalParam.RefreshParam(SRanipalTrack.LatestEyeData, SRanipalTrack.LatestLipData);
                
                if (QuickModeMenu.MainMenu != null) QuickModeMenu.MainMenu.UpdateParams(SRanipalTrack.LatestEyeData);

                yield return new WaitForSeconds(0.01f);
            }
        }
        
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        public override void OnUpdate()
        {
            if (MainThreadExecutionQueue.Count <= 0) return;
            
            MainThreadExecutionQueue[0].Invoke();
            MainThreadExecutionQueue.RemoveAt(0);
        }
    }
}
