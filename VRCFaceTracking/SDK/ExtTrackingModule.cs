using System;

namespace VRCFaceTracking
{
    public abstract class ExtTrackingModule
    {
        // Should UnifiedLibManager try to initialize this module if it's looking for a module that supports eye or lip.
        public virtual (bool SupportsEye, bool SupportsLip) Supported => (false, false);
        
        // Should the module be writing to UnifiedTrackingData for eye or lip tracking updates.
        public (ModuleState EyeState, ModuleState LipState) Status = (ModuleState.Uninitialized,
            ModuleState.Uninitialized);

        public abstract (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip);

        public abstract Action GetUpdateThreadFunc();

        public abstract void Teardown();
    }
}