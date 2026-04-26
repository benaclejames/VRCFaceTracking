using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.Core.Params
{
    public abstract class Parameter
    { 
        public abstract Parameter[] ResetParam(IParameterDefinition[] newParams);
        public abstract (string paramName, Parameter paramLiteral)[] GetParamNames();

        public virtual bool Deprecated => false;

        // Mark this parameter as needing a fresh emit on the next data tick. Default no-op;
        // overridden by leaf parameters (BaseParam<T>) to clear their value-equality cache so
        // the dedup short-circuit in the value setter does not swallow the next update.
        public virtual void MarkDirty() { }
    }
}