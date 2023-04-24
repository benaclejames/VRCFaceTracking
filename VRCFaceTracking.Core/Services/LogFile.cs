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

    public async void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        await semaphoreSlim.WaitAsync(); // Wait for the semaphore to be released
        try
        {
            await _file.WriteAsync($"[{_categoryName}] {logLevel}: {formatter(state, exception)}\n");
            await _file.FlushAsync();
        }
        finally
        {
            semaphoreSlim.Release(); // Release the semaphore
        }
    }
}