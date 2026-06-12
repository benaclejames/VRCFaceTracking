using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Services;

public class LogLine(string message, LogLevel level)
{
    public string Message { get; } = message;
    public LogLevel Level { get; } = level;

    public override string ToString() => Message;
}    

public class OutputPageLogger(string categoryName) : ILogger
{
    public static readonly ObservableCollection<LogLine> FilteredLogs = new();
    public static readonly ObservableCollection<LogLine> AllLogs = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if ( categoryName == "\0VRCFT\0" )
        {
            // Log events from sub-processes have the unique category name "\0VRCFT\0", so skip category name
            AddLineDispatched(new LogLine($"{formatter(state, exception)}", logLevel));
        }
        else
        { 
            AddLineDispatched(new LogLine($"[{categoryName}] {logLevel}: {formatter(state, exception)}", logLevel));
        }
    }

    private static void AddLineDispatched(LogLine line)
    {
        var dispatcher = Avalonia.Threading.Dispatcher.UIThread;
        if (dispatcher.CheckAccess())
        {
            AddLine(line);
        }
        else
        {
            dispatcher.Post(() => AddLine(line));
        }
    }

    private static void AddLine(LogLine line)
    {
        AllLogs.Add(line);
        if (line.Level >= LogLevel.Information)
        {
            FilteredLogs.Add(line);
        }
    }
}
