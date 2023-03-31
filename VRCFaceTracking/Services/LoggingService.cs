using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace VRCFaceTracking_Next.Services;
public class LoggingService
{
    public static ObservableCollection<string> Logs = new ();
    private static DispatcherQueue dispatcher;

    // We need to be adding all logs to the staticLog from the dispatcher thread
    public static void Setup(DispatcherQueue queue) => dispatcher = queue;


    public sealed class OutputLogProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, OutputLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new OutputLogger(name));

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class OutputLogger : ILogger
    {
        private readonly string _categoryName;

        public OutputLogger(string categoryName) => _categoryName = categoryName;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            // Add to the staticLog from the dispatcher thread
            dispatcher.TryEnqueue(() => LoggingService.Logs.Add($"[{_categoryName}] {logLevel}: {formatter(state, exception)}"));
        }
    }
}
