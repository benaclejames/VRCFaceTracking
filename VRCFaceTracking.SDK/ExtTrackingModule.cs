using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Library;

namespace VRCFaceTracking;

public abstract class ExtTrackingModule
{
    // Should UnifiedLibManager try to initialize this module if it's looking for a module that supports eye or lip.
    public virtual (bool SupportsEye, bool SupportsExpression) Supported => (false, false);

    public ModuleState Status = ModuleState.Uninitialized;

    public ILogger Logger;

    public ModuleMetadata ModuleInformation;

    public abstract (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable);

    public abstract void Update();

    public abstract void Teardown();
}