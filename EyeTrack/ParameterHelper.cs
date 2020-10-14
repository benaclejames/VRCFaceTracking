using VRC.SDK3.Avatars.ScriptableObjects;

namespace EyeTrack
{
    public static class ParameterHelper
    {
        public static int GetParamIndex(string paramName)
        {
            VRCExpressionParameters.Parameter[] parameters = new VRCExpressionParameters.Parameter[0];

            if (VRCPlayer
                .field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0?.prop_VRCAvatarDescriptor_0?.expressionParameters?.parameters != null)
            {
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
        
        public static void SetParameter(AvatarAnimParamController controller,
            MainMod.AV3Parameter param, float value)
        {
            if (controller == null || controller.field_Private_AvatarPlayableController_0 == null ||
                param.ParamEnum == AvatarPlayableController.EnumNPublicSealedvaStNoSt18StStStStStUnique.None)
                return;
            controller.field_Private_AvatarPlayableController_0
                .Method_Public_Void_EnumNPublicSealedvaStNoSt18StStStStStUnique_Single_0(param.ParamEnum, value);
        }
    }
}