using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VRCEyeTracking.ParamLib
{
    public static class ParamLib
    {
        private static AvatarPlayableController GetLocalPlayableController() => VRCPlayer.field_Internal_Static_VRCPlayer_0
            ?.field_Private_VRC_AnimationController_0
            ?.field_Private_AvatarAnimParamController_0
            ?.field_Private_AvatarPlayableController_0;
        
        private static AvatarAnimParamController GetLocalAnimParamController() => VRCPlayer.field_Internal_Static_VRCPlayer_0
            ?.field_Private_VRC_AnimationController_0?.field_Private_AvatarAnimParamController_0;
        
        public static void PrioritizeParameter(int? paramIndex)
        {
            if (paramIndex == null) return;
            
            var controller = GetLocalPlayableController();
            if (controller == null) return;
            
            controller.Method_Public_Void_Int32_0((int)paramIndex);
        }
        
        public static int? GetParamIndex(string paramName)
        {
            VRCExpressionParameters.Parameter[] parameters = VRCPlayer.field_Internal_Static_VRCPlayer_0
                ?.prop_VRCAvatarManager_0?.prop_VRCAvatarDescriptor_0?.expressionParameters
                ?.parameters;

            if (parameters == null)
                return null;

            int? index = null;
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.name == null)
                    return null;
                if (param.name == paramName) index = i;
            }

            return index;
        }
        
        public static bool SetParameter(int? paramIndex, float value)
        {
            if (paramIndex == null) return false;

            var controller = GetLocalAnimParamController();
            if (controller?.field_Private_AvatarPlayableController_0 == null) return false;

            controller.field_Private_AvatarPlayableController_0.Method_Public_Boolean_Int32_Single_1((int)paramIndex, value);
            return true;
        }
    }
}