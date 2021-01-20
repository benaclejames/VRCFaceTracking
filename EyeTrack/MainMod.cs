using System.Collections;
using System.Collections.Generic;
using EyeTrack;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(MainMod), "EyeTrack", "1.2.0", "Benaclejames",
    "https://github.com/benaclejames/VRCEyeTracking")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace EyeTrack
{
    public class MainMod : MelonMod
    {
        // Param Names
        public static List<AV3Parameter> eyeTrackParams = EmptyList();

        public static List<AV3Parameter> EmptyList()
        {
            return new List<AV3Parameter>
            {
                new AV3Parameter {ParamName = "EyesX", Prioritize = true},
                new AV3Parameter {ParamName = "EyesY", Prioritize = true},
                new AV3Parameter {ParamName = "LeftEyeLid", Prioritize = true},
                new AV3Parameter {ParamName = "RightEyeLid", Prioritize = true},
                new AV3Parameter {ParamName = "EyesWiden"},
                new AV3Parameter {ParamName = "EyesDilation"},
                new AV3Parameter {ParamName = "LeftEyeX", Prioritize = true},
                new AV3Parameter {ParamName = "LeftEyeY", Prioritize = true},
                new AV3Parameter {ParamName = "RightEyeX", Prioritize = true},
                new AV3Parameter {ParamName = "RightEyeY", Prioritize = true},
                new AV3Parameter {ParamName = "LeftEyeWiden"},
                new AV3Parameter {ParamName = "RightEyeWiden"},
                new AV3Parameter {ParamName = "LeftEyeSqueeze"},
                new AV3Parameter {ParamName = "RightEyeSqueeze"}
            };
        }

        public override void VRChat_OnUiManagerInit()
        {
            SRanipalTrack.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdatePriority());
            MelonCoroutines.Start(UpdateParams());
            MelonLogger.Log("SRanipal SDK Started. Eye Tracking Active");
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

        //EnumNPublicSealedvaUnBoInFl5vUnique
        
        private static void SetPriority(AvatarPlayableController controller, bool priority,
            int paramIndex)
        {
            if (paramIndex == -1)
                return;

            var param = ParameterHelper.GetParam(VRCPlayer.field_Internal_Static_VRCPlayer_0
                .field_Private_VRC_AnimationController_0.field_Private_AvatarAnimParamController_0, paramIndex);

            /*param.prop_Boolean_0 = true;
            param.field_Private_Boolean_0 = true;
            param.field_Public_Boolean_0 = true;*/
        }

        private static IEnumerator UpdatePriority()
        {
            for (;;)
            {
                yield return new WaitForSeconds(5);
                    if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.field_Private_VRC_AnimationController_0
                        ?.field_Private_AvatarAnimParamController_0?.field_Private_AvatarPlayableController_0 == null)
                        continue;

                    var controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0
                        .field_Private_AvatarAnimParamController_0
                        .field_Private_AvatarPlayableController_0;

                    foreach (var param in eyeTrackParams.ToArray())
                        if (param.Prioritize)
                            SetPriority(controller, true, param.ParamIndex);
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

                    foreach (var param in eyeTrackParams.ToArray())
                        ParameterHelper.SetParameter(controller, param, SRanipalTrack.SRanipalData[param.ParamName]);

                yield return new WaitForSeconds(0.01f);
            }
        }

        public static void ScanForParamEnums()
        {
            for (var i = 0; i < eyeTrackParams.ToArray().Length; i++)
            {
                var param = eyeTrackParams[i];
                param.ParamIndex = ParameterHelper.GetParamIndex(param.ParamName);
                eyeTrackParams[i] = param;
            }
        }

        public struct AV3Parameter
        {
            public string ParamName;
            public int ParamIndex;
            public bool Prioritize;
        }
    }
}