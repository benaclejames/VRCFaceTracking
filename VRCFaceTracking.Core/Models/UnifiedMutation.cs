using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Models;

public class UnifiedMutationProperty
{
    public string Name { get; set; }
    public object Value { get; set; }
}

public interface IUnifiedMutation
{
    string Name { get; }
    int Order { get; }
    bool Mutable { get; set; }
    void Mutate(ref UnifiedTrackingData data, UnifiedTrackingData buffer, ILogger<UnifiedTrackingMutator> _logger);
    UnifiedMutationProperty[] GetProperties();
    void SetProperties(UnifiedMutationProperty[] props);
    void Initialize();
    void Reset();
}
