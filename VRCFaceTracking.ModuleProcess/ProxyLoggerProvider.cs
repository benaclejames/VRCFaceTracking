using System.Threading;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.ModuleProcess;

public class ProxyLoggerProvider(SynchronizationContext? syncContext = null) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new ProxyLogger(categoryName, syncContext);
    }

    public void Dispose() { }
}
