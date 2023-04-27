using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Services;

public class LogFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly StreamWriter _file;
    private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);

    public LogFileLogger(string categoryName, StreamWriter file)
    {
        _categoryName = categoryName;
        _file = file;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        semaphoreSlim.Wait(); // Wait for the semaphore to be released
        try
        {
            _file.Write($"[{_categoryName}] {logLevel}: {formatter(state, exception)}\n");
            _file.Flush();
        }
        finally
        {
            semaphoreSlim.Release(); // Release the semaphore
        }
    }
}