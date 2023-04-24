using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace VRCFaceTracking.Services;

public sealed class OutputLogProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, OutputPageLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    
    private readonly DispatcherQueue _dispatcher;
    
    public OutputLogProvider(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public ILogger CreateLogger(string categoryName) => 
        _loggers.GetOrAdd(categoryName, name => new OutputPageLogger(name, _dispatcher));

    public void Dispose() => _loggers.Clear();
}