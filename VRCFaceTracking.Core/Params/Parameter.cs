using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.Params
{
    public abstract class Parameter
    { 
        public abstract Parameter[] ResetParam(IParameterDefinition[] newParams, IAvatarInfo avatarInfo);
        public abstract (string paramName, Parameter paramLiteral)[] GetParamNames();

        public virtual bool Deprecated => false;
    }
}