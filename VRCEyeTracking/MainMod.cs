using System.Collections;
using System.Collections.Generic;
using VRCEyeTracking;
using MelonLoader;
using UnityEngine;
using VRCEyeTracking.QuickMenu;
using VRCEyeTracking.SRParam;

[assembly: MelonInfo(typeof(MainMod), "VRCEyeTracking", "1.3.0", "benaclejames",
    "https://github.com/benaclejames/VRCEyeTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCEyeTracking
{
    public class MainMod : MelonMod
    {
        public static void ResetParams() => EyeTrackParams.ForEach(param => param.ResetParam());
        public static void ZeroParams() => EyeTrackParams.ForEach(param => param.ZeroParam());

        private static readonly List<ISRanipalParam> EyeTrackParams = new List<ISRanipalParam>
        {
            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.combined.eye_data.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "EyesX", "EyesY"),
            
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide >
                                                  v2.expression_data.right.eye_wide
                ? v2.expression_data.left.eye_wide
                : v2.expression_data.right.eye_wide, "EyesWiden"),
            
            new SRanipalGeneralEyeParameter(v2 =>
            {
                var normalizedFloat = SRanipalTrack.CurrentDiameter / SRanipalTrack.MinOpen / (SRanipalTrack.MaxOpen - SRanipalTrack.MinOpen);
                return Mathf.Clamp(normalizedFloat, 0, 1);
            }, "EyesDilation"),
            
            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.left.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "LeftEyeX", "LeftEyeY"),
            
            new SRanipalXYEyeParameter(v2 => Vector3.Scale(
                v2.verbose_data.right.gaze_direction_normalized,
                new Vector3(-1, 1, 1)), "RightEyeX", "RightEyeY"),
            
            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.left.eye_openness, "LeftEyeLid", true),
            new SRanipalGeneralEyeParameter(v2 => v2.verbose_data.right.eye_openness, "RightEyeLid", true),
            
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.left.eye_wide, "LeftEyeWiden"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_wide, "RightEyeWiden"),
            
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_squeeze, "LeftEyeSqueeze"),
            new SRanipalGeneralEyeParameter(v2 => v2.expression_data.right.eye_squeeze, "RightEyeSqueeze"),
        };

        public override void OnApplicationStart() => DependencyManager.Init();

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdateParams());
            MelonLogger.Msg("SRanipal SDK Started. Eye Tracking Active");
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

        private static IEnumerator UpdateParams()
        {
            for (;;)
            {
                foreach (var param in EyeTrackParams.ToArray())
                    param.RefreshParam(SRanipalTrack.LatestEyeData, null);

                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}