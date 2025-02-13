using VRCFaceTracking.Models;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace VRCFaceTracking.Services;

public class OutputPageLogger : ILogger
{
    private readonly string _categoryName;
    public static readonly ObservableCollection<string> FilteredLogs = new();
    public static readonly ObservableCollection<string> AllLogs = new();
    public static ObservableCollection<LoggingContext> ErrorLogs = new();
    private static DispatcherQueue? _dispatcher;

    public OutputPageLogger(string categoryName, DispatcherQueue? queue)
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
        _dispatcher?.TryEnqueue(() =>
        {
            // Add to the staticLog from the dispatcher thread
            var log = $"[{_categoryName}] {logLevel}: {formatter(state, exception)}";
            AllLogs.Add(log);
            // Filtered is what the user sees, so show Information scope
            if (logLevel >= LogLevel.Information)
            {
                FilteredLogs.Add(log);
            }

            if (logLevel == LogLevel.Error)
            {
                var skimmedLog = $"{formatter(state, exception)}";

                // Find the matching log context
                var existingLog = ErrorLogs.FirstOrDefault(item => item.Context == _categoryName);

                if (existingLog != null)
                {
                    if (!existingLog.Logs.Contains(skimmedLog))
                    {
                        existingLog.Logs += $"\n{skimmedLog}";
                    }
                }
                else
                {
                    ErrorLogs.Add(new LoggingContext { Context = _categoryName, Logs = skimmedLog });
                }
            }
        });
    }
}