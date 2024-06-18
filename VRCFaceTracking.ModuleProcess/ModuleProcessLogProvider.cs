using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace VRCFaceTracking.ModuleProcess;
public class ModuleProcessLogProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ModuleProcessLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly DispatcherQueue _dispatcher;

    public ModuleProcessLogProvider(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new ModuleProcessLogger(name, _dispatcher));

    public void Dispose() => _loggers.Clear();
}
