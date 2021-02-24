using VRC.SDK3.Avatars.ScriptableObjects;

namespace VRCEyeTracking.ParamLib
{
    public static class ParamLib
    {
        private static AvatarPlayableController GetLocalPlayableController() => VRCPlayer.field_Internal_Static_VRCPlayer_0
            .field_Private_VRC_AnimationController_0
            .field_Private_AvatarAnimParamController_0
            .field_Private_AvatarPlayableController_0;
        
        private static AvatarAnimParamController GetLocalAnimParamController() => VRCPlayer.field_Internal_Static_VRCPlayer_0
            .field_Private_VRC_AnimationController_0.field_Private_AvatarAnimParamController_0;
        
        public static bool PrioritizeParameter(int paramIndex)
        {
            if (paramIndex == -1) return false;
            
            var controller = GetLocalPlayableController();
            if (controller == null) return false;
            
            controller.Method_Public_Void_Int32_0(paramIndex);
            return true;
        }
        
        public static int GetParamIndex(string paramName)
        {
            VRCExpressionParameters.Parameter[] parameters;

            if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0?.prop_VRCAvatarDescriptor_0
                ?.expressionParameters?.parameters != null)
                parameters = VRCPlayer.field_Internal_Static_VRCPlayer_0
                    .prop_VRCAvatarManager_0.prop_VRCAvatarDescriptor_0.expressionParameters
                    .parameters;
            else
                return -1;

            var index = -1;
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (param.name == null)
                    return -1;
                if (param.name == paramName) index = i;
            }

            return index;
        }
        
        public static bool SetParameter(int paramIndex, float value)
        {
            if (paramIndex == -1) return false;

            var controller = GetLocalAnimParamController();
            if (controller?.field_Private_AvatarPlayableController_0 == null) return false;

            controller.field_Private_AvatarPlayableController_0.Method_Public_Boolean_Int32_Single_1(paramIndex, value);
            return true;
        }
    }
}