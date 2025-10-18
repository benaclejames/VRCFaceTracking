using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace VRCFaceTracking.ModuleProcess;
public class ProxyLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ProxyLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly DispatcherQueue _dispatcher;

    public ProxyLoggerProvider(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new ProxyLogger(name, _dispatcher));

    public void Dispose() => _loggers.Clear();
}
