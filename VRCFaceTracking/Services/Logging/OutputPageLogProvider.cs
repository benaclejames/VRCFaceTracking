using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Services.Logging;

public sealed class OutputPageLogProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputPageLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) => 
        _loggers.GetOrAdd(categoryName, name => new OutputPageLogger(name));

    public void Dispose() => _loggers.Clear();
}