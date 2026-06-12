using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Services;

public sealed class OutputLogProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputPageLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) => 
        _loggers.GetOrAdd(categoryName, name => new OutputPageLogger(name));

    public void Dispose() => _loggers.Clear();
}