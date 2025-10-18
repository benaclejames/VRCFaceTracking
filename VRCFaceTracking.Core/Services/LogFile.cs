using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Services;

public class LogFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly StreamWriter _file;
    private static readonly Mutex Mutex = new ();

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
        Mutex.WaitOne(); // Wait for the semaphore to be released
        try
        {
            // Log events from sub-processes have the unique category name "\0VRCFT\0"
            if ( _categoryName == "\0VRCFT\0" )
            {
                _file.Write($"{formatter(state, exception)}\n");
            }
            else
            {
                _file.Write($"[{_categoryName}] {logLevel}: {formatter(state, exception)}\n");
            }
            _file.Flush();
        }
        catch
        {
            // Ignore cus sandboxing causes a lot of issues here
        }
        
        Mutex.ReleaseMutex(); // Release the semaphore
    }
}