using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using VRCEyeTracking;
using MelonLoader;
using UnityEngine;
using VRCEyeTracking.ParamLib;

[assembly: MelonInfo(typeof(MainMod), "VRCEyeTracking", "1.2.3", "Benaclejames",
    "https://github.com/benaclejames/VRCEyeTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCEyeTracking
{
    public class MainMod : MelonMod
    {
        public static List<FloatParam> EyeTrackParams = new List<FloatParam>();

        public static List<FloatParam> EmptyList()
        {
            return new List<FloatParam>
            {
                new FloatParam("EyesX", true),
                new FloatParam("EyesY", true),
                new FloatParam("LeftEyeLid", true),
                new FloatParam("RightEyeLid", true),
                new FloatParam("EyesWiden"),
                new FloatParam("EyesDilation"),
                new FloatParam("LeftEyeX", true),
                new FloatParam("LeftEyeY", true),
                new FloatParam("RightEyeX", true),
                new FloatParam("RightEyeY", true),
                new FloatParam("LeftEyeWiden"),
                new FloatParam("RightEyeWiden"),
                new FloatParam("LeftEyeSqueeze"),
                new FloatParam("RightEyeSqueeze")
            };
        }

        public override void OnApplicationStart() => DependencyManager.Init();

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdatePriority());
            MelonCoroutines.Start(UpdateParams());
            MelonLogger.Msg("SRanipal SDK Started. Eye Tracking Active");
        }

        public override void OnApplicationQuit()
        {
            SRanipalTrack.Stop();
        }

        public override void OnLevelWasInitialized(int level)
        {
            SRanipalTrack.MinOpen = 999;
            SRanipalTrack.MaxOpen = 0;
        }

        private static IEnumerator UpdatePriority()
        {
            for (;;)
            {
                yield return new WaitForSeconds(5);
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0 != null) continue;

                foreach (var param in EyeTrackParams.ToArray())
                    if (param.Prioritised)
                        param.Prioritised = true;
            }
        }

        private static IEnumerator UpdateParams()
        {
            for (;;)
            {
                AvatarAnimParamController controller = null;
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.field_Private_VRC_AnimationController_0
                    ?.field_Private_AvatarAnimParamController_0 != null)
                    controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0.field_Private_AvatarAnimParamController_0;
                if (controller == null)
                {
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                foreach (var param in EyeTrackParams.ToArray())
                    param.ParamValue = SRanipalTrack.SRanipalData[param.ParamName];

                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}