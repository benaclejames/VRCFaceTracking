using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Models;

public interface IUnifiedMutation
{
    string Name { get; }
    int Order { get; }
    bool Mutable { get; set; }
    void Mutate(ref UnifiedTrackingData data, UnifiedTrackingData buffer, ILogger<UnifiedTrackingMutator> _logger);
    object GetProperties();
    void SetProperties(object data);
    void Initialize();
    void Reset();
}
