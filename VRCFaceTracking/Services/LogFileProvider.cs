using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace VRCFaceTracking.Core.Services;

[ProviderAlias("Debug")]
public class LogFileProvider : ILoggerProvider
{
    private readonly FileStream _file;
    private readonly StreamWriter _writer;
    
    public LogFileProvider()
    {
        if (!Directory.Exists(Utils.PersistentDataDirectory))   // Eat my ass windows
            Directory.CreateDirectory(Utils.PersistentDataDirectory);
    
        var logPath = Path.Combine(Utils.PersistentDataDirectory, "latest" + ".log");
        _file = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
        _writer = new StreamWriter(_file);
    }
    
    private readonly ConcurrentDictionary<string, LogFileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new LogFileLogger(name, _writer));

    public void Dispose() => _loggers.Clear();
}