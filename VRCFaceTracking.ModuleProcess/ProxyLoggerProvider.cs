using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.ModuleProcess;
public class ProxyLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ProxyLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new ProxyLogger(name));

    public void Dispose() => _loggers.Clear();
}
