using System;
using System.Collections;
using System.Collections.Generic;
using VRCEyeTracking;
using MelonLoader;
using UnityEngine;
using VRCEyeTracking.QuickMenu;
using VRCEyeTracking.SRParam;
using VRCEyeTracking.SRParam.LipMerging;

[assembly: MelonInfo(typeof(MainMod), "VRCEyeTracking", "1.3.0", "benaclejames",
    "https://github.com/benaclejames/VRCEyeTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCEyeTracking
{
    public class MainMod : MelonMod
    {
        public static void ResetParams() => SRanipalTrackParams.ForEach(param => param.ResetParam());
        public static void ZeroParams() => SRanipalTrackParams.ForEach(param => param.ZeroParam());

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
                    unoptimizedShape.ToString()));
        }

        public static void AppendEyeParams() => SRanipalTrackParams.AddRange(EyeTrackingParams.ParameterList);

        public override void OnApplicationStart()
        {
            DependencyManager.Init();
        }

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Initializer.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdateParams());
            MelonCoroutines.Start(CheckExecutionQueue());
        }

        public override void OnApplicationQuit()
        {
            SRanipalTrack.Stop();
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
                SRanipalTrackParams.ForEach(param => param.RefreshParam(SRanipalTrack.LatestEyeData, SRanipalTrack.LatestLipData));

                yield return new WaitForSeconds(0.01f);
            }
        }
        
        public static readonly List<Action> MainThreadExecutionQueue = new List<Action>();

        private static IEnumerator CheckExecutionQueue()
        {
            for (;;)
            {
                if (MainThreadExecutionQueue.Count > 0)
                {
                    MainThreadExecutionQueue[0].Invoke();
                    MainThreadExecutionQueue.RemoveAt(0);
                } 

                yield return new WaitForSeconds(5f);
            } 
        }
    }
}