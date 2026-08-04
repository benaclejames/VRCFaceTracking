using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Services.Logging;

public class BufferLogger(string category, LogBufferProvider owner) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
    public bool IsEnabled(LogLevel level) => true;

    public void Log<TState>(LogLevel level, EventId id, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var msg = category == "\0VRCFT\0"
            ? formatter(state, exception)
            : $"[{category}] {level}: {formatter(state, exception)}";
        owner.Append(msg);
    }
}