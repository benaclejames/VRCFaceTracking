using System.Collections;
using System.Collections.Generic;
using EyeTrack;
using MelonLoader;
using UnityEngine;


namespace EyeTrack
{
    public class MainMod : MelonMod
    {
        // Param Names
        public static List<AV3Parameter> eyeTrackParams = EmptyList();

        private readonly SRanipalTrack _tracker = new SRanipalTrack();

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
                new AV3Parameter {ParamName = "RightEyeWiden"}
            };
        }

        public override void VRChat_OnUiManagerInit()
        {
            _tracker.Start();
            Hooking.SetupHooking();
            MelonCoroutines.Start(UpdatePriority());
            MelonCoroutines.Start(UpdateParams());
            MelonLogger.Log("SRanipal SDK Started. Eye Tracking Active");
        }

        public override void OnApplicationQuit()
        {
            _tracker.Stop();
        }

        public override void OnLevelWasInitialized(int level)
        {
            _tracker.MinOpen = 999;
            _tracker.MaxOpen = 0;
        }

        private static void SetPriority(AvatarPlayableController controller, bool priority,
            AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique paramEnum)
        {
            if (paramEnum == AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None)
                return;

            controller.Method_Public_Void_Int32_Boolean_0((int) paramEnum, priority);
            controller.Method_Public_Boolean_Int32_Boolean_PDM_0((int) paramEnum, priority);
            controller.Method_Public_Void_Int32_Boolean_1((int) paramEnum, priority);
            controller.Method_Public_Void_Int32_Boolean_2((int) paramEnum, priority);
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

                foreach (var param in eyeTrackParams)
                    if (param.Prioritize)
                        SetPriority(controller, true, param.ParamEnum);
            }
        }

        private IEnumerator UpdateParams()
        {
            for (;;)
            {
                AvatarAnimParamController controller = null;
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.field_Private_VRC_AnimationController_0
                    ?.field_Private_AvatarAnimParamController_0 != null)
                    controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0.field_Private_AvatarAnimParamController_0;
                else
                    yield return new WaitForSeconds(2);
                if (controller == null)
                    yield return new WaitForSeconds(2);

                foreach (var param in eyeTrackParams)
                    ParameterHelper.SetParameter(controller, param, _tracker.SRanipalData[param.ParamName]);

                yield return new WaitForFixedUpdate();
            }
        }

        private static AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique GetParam(string name)
        {
            var paramIndex = ParameterHelper.GetParamIndex(name);
            return paramIndex != -1
                ? (AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique) paramIndex
                : AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None;
        }

        public static void ScanForParamEnums()
        {
            for (var i = 0; i < eyeTrackParams.Count; i++)
            {
                var param = eyeTrackParams[i];
                param.ParamEnum = GetParam(param.ParamName);
                eyeTrackParams[i] = param;
            }
        }

        public struct AV3Parameter
        {
            public string ParamName;
            public AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique ParamEnum;
            public bool Prioritize;
        }
    }
}