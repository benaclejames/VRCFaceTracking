using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Services.Logging;

public class LogBufferProvider : ILoggerProvider
{
    private const int MaxLines = 10_000;
    private readonly ConcurrentQueue<string> _lines = new();

    private readonly ConcurrentDictionary<string, BufferLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new BufferLogger(name, this));

    public string Snapshot()
    {
        var sb = new StringBuilder();
        foreach (var line in _lines)
            sb.AppendLine(line);
        return sb.ToString();
    }

    internal void Append(string line)
    {
        _lines.Enqueue(line);
        while (_lines.Count > MaxLines && _lines.TryDequeue(out _))
        {
        }
    }

    public void Dispose() => _loggers.Clear();
}