using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace VRCFaceTracking.Services;
    
public class OutputPageLogger : ILogger
{
    private readonly string _categoryName;
    public static readonly ObservableCollection<string> FilteredLogs = new();
    public static readonly ObservableCollection<string> AllLogs = new();
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
        // Add to the staticLog from the dispatcher thread
        _dispatcher?.TryEnqueue(() =>
        {
            if ( _categoryName == "\0VRCFT\0" )
            {
                // Log events from sub-processes have the unique category name "\0VRCFT\0"
                AllLogs.Add($"{formatter(state, exception)}");
                // Filtered is what the user sees, so show Information scope
                if ( logLevel >= LogLevel.Information )
                {
                    FilteredLogs.Add($"{formatter(state, exception)}");
                }
            }
            else
            {
                AllLogs.Add($"[{_categoryName}] {logLevel}: {formatter(state, exception)}");
                // Filtered is what the user sees, so show Information scope
                if ( logLevel >= LogLevel.Information )
                {
                    FilteredLogs.Add($"[{_categoryName}] {logLevel}: {formatter(state, exception)}");
                }
            }
        });
    }
}