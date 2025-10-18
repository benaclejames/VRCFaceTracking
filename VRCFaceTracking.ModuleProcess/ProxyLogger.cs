using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace VRCFaceTracking.ModuleProcess;

public delegate void OnLog(LogLevel level, string msg);

public class ProxyLogger : ILogger
{
    private readonly string _categoryName;
    // public static readonly ObservableCollection<string> AllLogs = new();
    public static OnLog OnLog;
    private static DispatcherQueue? _dispatcher;

    public ProxyLogger(string categoryName, DispatcherQueue? queue)
    {
        _categoryName = categoryName;
        _dispatcher = queue;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if ( OnLog != null )
        {
            OnLog(logLevel, $"[{_categoryName}] {logLevel}: {formatter(state, exception)}");
        }
    }
}
