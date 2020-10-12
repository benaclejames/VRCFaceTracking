using System.Collections;
using System.Threading.Tasks;
using Il2CppSystem;
using MelonLoader;
using UnhollowerBaseLib;
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
            MelonCoroutines.Start(UpdateParamEnums());
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

        private static int GetParamIndex(string paramName)
        {
            VRCExpressionParameters.Parameter[] parameters = new VRCExpressionParameters.Parameter[0];

            if (VRCPlayer
                .field_Internal_Static_VRCPlayer_0 != null)
            {
                if (VRCPlayer
                    .field_Internal_Static_VRCPlayer_0
                    .prop_VRCAvatarManager_0 != null)
                    if (VRCPlayer
                        .field_Internal_Static_VRCPlayer_0
                        .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0 != null)
                        if (VRCPlayer
                            .field_Internal_Static_VRCPlayer_0
                            .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0.expressionParameters != null)
                            if (VRCPlayer
                                    .field_Internal_Static_VRCPlayer_0
                                    .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0.expressionParameters
                                    .parameters !=
                                null)
                                parameters = VRCPlayer
                                    .field_Internal_Static_VRCPlayer_0
                                    .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0.expressionParameters
                                    .parameters;

            }
            else
                return -1;
            
            var index = -1;
            for (var i = 0; i < parameters.Length; i++)
            {
                VRCExpressionParameters.Parameter param = parameters[i];
                if (param.name == null)
                    return -1;
                if (param.name == paramName)
                {
                    index = i;
                }
            }

            return index;
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
            param param, float value)
        {
            if (controller == null || controller.field_Private_AvatarPlayableController_0 == null ||
                param.paramEnum == AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None)
                return;
            controller.field_Private_AvatarPlayableController_0
                .Method_Public_Void_EnumNPublicSealedvaStNoSt18StStStStStUnique_Single_0(param.paramEnum, value);
        }

        IEnumerator UpdateParams()
        {
            for (;;)
            {
                AvatarAnimParamController controller = null;
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0 != null)
                {
                    if (VRCPlayer.field_Internal_Static_VRCPlayer_0
                        .field_Private_VRC_AnimationController_0 != null)
                        if (VRCPlayer.field_Internal_Static_VRCPlayer_0
                            .field_Private_VRC_AnimationController_0
                            .field_Private_AvatarAnimParamController_0)
                            controller = VRCPlayer.field_Internal_Static_VRCPlayer_0
                                .field_Private_VRC_AnimationController_0.field_Private_AvatarAnimParamController_0;
                }
                else
                    yield return new WaitForSeconds(2);
                if (controller == null)
                    yield return new WaitForSeconds(2);
                
                SetParameter(controller, leftEyeLid, _tracker.LeftOpenness);
                SetParameter(controller, rightEyeLid, _tracker.RightOpenness);
                SetParameter(controller, eyesWiden, _tracker.CombinedWiden);
                SetParameter(controller, eyesX, _tracker.GazeDirectionCombinedLocal.x);
                SetParameter(controller, eyesY, _tracker.GazeDirectionCombinedLocal.y);

                float normalizedFloat = (_tracker.Diameter / _tracker.MinOpen) / (_tracker.MaxOpen - _tracker.MinOpen);
                normalizedFloat = Mathf.Clamp(normalizedFloat, 0, 1);
                SetParameter(controller, eyesDilation, normalizedFloat);
                
                yield return new WaitForFixedUpdate();
            }
        }

        struct param
        {
            public AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique paramEnum;
            public int paramIndex;
        }
        
        private param eyesX,
            eyesY,
            leftEyeLid,
            rightEyeLid,
            eyesWiden,
            eyesDilation;

        param GetParam(string name)
        {
            var paramIndex = GetParamIndex(name);
            return new param
            {
                paramIndex = paramIndex,
                paramEnum = paramIndex != -1 ? (AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique) paramIndex : AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None
            };
        }
        
        IEnumerator UpdateParamEnums()
        {
            for (;;) {
                eyesX = GetParam("EyesX");
                eyesY = GetParam("EyesY");
                leftEyeLid = GetParam("LeftEyeLid");
                rightEyeLid = GetParam("RightEyeLid");
                eyesWiden = GetParam("EyesWiden");
                eyesDilation = GetParam("EyesDilation");
                yield return new WaitForSeconds(3f);
            }
        }
    }
}