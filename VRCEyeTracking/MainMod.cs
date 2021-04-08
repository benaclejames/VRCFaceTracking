using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRCEyeTracking;
using MelonLoader;
using UnityEngine;
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

        public static readonly List<ISRanipalParam> SRanipalTrackParams = new List<ISRanipalParam>();
        
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
        }

        public override void OnApplicationQuit()
        {
            SRanipalTrack.Stop();
        }

        public override void OnSceneWasLoaded(int level, string levelName)
        {
            //if (level == -1 && !QuickModeMenu.HasInitMenu)
            //    QuickModeMenu.InitializeMenu();
            
            SRanipalTrack.MinOpen = 999;
            SRanipalTrack.MaxOpen = 0;
        }
    }
}