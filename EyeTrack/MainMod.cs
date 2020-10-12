using System.Collections;
using MelonLoader;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEngine;

namespace EyeTrack
{
    public class MainMod : MelonMod
    {

        private readonly SRanipalTrack _tracker = new SRanipalTrack();

        public override void VRChat_OnUiManagerInit()
        {
            _tracker.Start();
            MelonCoroutines.Start(UpdatePriority());
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

        private static int GetParamIndex(string paramName)
        {
            var parameters = VRCPlayer
                .field_Internal_Static_VRCPlayer_0
                .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0.expressionParameters.parameters;

            var index = -1;

            for (var i = 0; i < parameters.Length; i++)
            {
                VRCExpressionParameters.Parameter param = parameters[i];
                if (param.name == paramName)
                {
                    index = i;
                }
            }

            return index;
        }

        private AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique GetParamEnum(string paramName)
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0 == null || VRCPlayer
                .field_Internal_Static_VRCPlayer_0
                .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0 == null)
                return AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None;

            int paramIndex = GetParamIndex(paramName);
            if (paramIndex == -1)
                return AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None;

            var foundParam =
                (AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique) paramIndex;
            return foundParam;
        }

        private void SetPriority(AvatarPlayableController controller, bool priority, string paramName)
        {
            int paramIndex = GetParamIndex(paramName);
            if (paramIndex == -1)
                return;
            controller.Method_Public_Void_Int32_Boolean_0(paramIndex, priority);
            controller.Method_Public_Boolean_Int32_Boolean_PDM_0(paramIndex, priority);
            controller.Method_Public_Void_Int32_Boolean_1(paramIndex, priority);
            controller.Method_Public_Void_Int32_Boolean_2(paramIndex, priority);
        }

        private IEnumerator UpdatePriority()
        {
            for (;;)
            {
                try
                {
                    if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null || VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0 == null)
                        break;

                    AvatarPlayableController controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0
                        .field_Private_AvatarAnimParamController_0
                        .field_Private_AvatarPlayableController_0;

                    SetPriority(controller, true, "EyesX");
                    SetPriority(controller, true, "EyesY");
                    SetPriority(controller, true, "LeftEyeLid");
                    SetPriority(controller, true, "RightEyeLid");
                }
                catch
                {
                    MelonLogger.LogError("Error occured upgrading parameter priority");
                }

                yield return new WaitForSeconds(3);
            }
        }

        void SetParameter(AvatarAnimParamController controller,
            AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique param, float value)
        {
            if (controller.field_Private_AvatarPlayableController_0 == null ||
                param == AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None)
                return;
            controller.field_Private_AvatarPlayableController_0
                .Method_Public_Void_EnumNPublicSealedvaStNoSt18StStStStStUnique_Single_0(param, value);
        }

        public override void OnUpdate()
        {
            if (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null || VRCPlayer.field_Internal_Static_VRCPlayer_0
                .field_Private_VRC_AnimationController_0 == null)
                return;

            AvatarAnimParamController controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                .field_Private_VRC_AnimationController_0
                .field_Private_AvatarAnimParamController_0;

            var eyesX = GetParamEnum("EyesX");
            var eyesY = GetParamEnum("EyesY");
            var leftEyeLid = GetParamEnum("LeftEyeLid");
            var rightEyeLid = GetParamEnum("RightEyeLid");
            var eyesWiden = GetParamEnum("EyesWiden");
            var eyesDilation = GetParamEnum("EyesDilation");

            SetParameter(controller, leftEyeLid, _tracker.LeftOpenness);
            SetParameter(controller, rightEyeLid, _tracker.RightOpenness);
            SetParameter(controller, eyesWiden, _tracker.CombinedWiden);
            SetParameter(controller, eyesX, _tracker.GazeDirectionCombinedLocal.x);
            SetParameter(controller, eyesY, _tracker.GazeDirectionCombinedLocal.y);

            float normalizedFloat = (_tracker.Diameter / _tracker.MinOpen) / (_tracker.MaxOpen - _tracker.MinOpen);
            normalizedFloat = Mathf.Clamp(normalizedFloat, 0, 1);
            SetParameter(controller, eyesDilation, normalizedFloat);
        }
    }
}