using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia.Threading;
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

    private static readonly ConcurrentQueue<LogLine> _pending = new();
    private static DispatcherTimer? _flushTimer;
    private static int _timerStarted;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var line = categoryName == "\0VRCFT\0"
            // Log events from sub-processes have the unique category name "\0VRCFT\0", so skip category name
            ? new LogLine($"{formatter(state, exception)}", logLevel)
            : new LogLine($"[{categoryName}] {logLevel}: {formatter(state, exception)}", logLevel);

        _pending.Enqueue(line);
        EnsureFlushTimer();
    }

    private static void EnsureFlushTimer()
    {
        if (Interlocked.CompareExchange(ref _timerStarted, 1, 0) != 0)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            _flushTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Background,
                Flush);
            _flushTimer.Start();
        });
    }

    private static void Flush(object? sender, EventArgs e)
    {
        while (_pending.TryDequeue(out var line))
        {
            AllLogs.Add(line);
            if (line.Level >= LogLevel.Information)
                FilteredLogs.Add(line);
        }
    }
}
