using System;

namespace VRCFaceTracking
{
    public abstract class ExtTrackingModule
    {
        // Should UnifiedLibManager try to initialize this module if it's looking for a module that supports eye or lip.
        public virtual (bool SupportsEye, bool SupportsExpression) Supported => (false, false);

        // Should the module be writing to UnifiedTrackingData for eye or lip tracking updates.
        public (ModuleState EyeState, ModuleState ExpressionState) Status = (ModuleState.Uninitialized,
            ModuleState.Uninitialized);

        public abstract (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable);

        public abstract Action GetUpdateThreadFunc();

        public abstract void Teardown();
    }
}