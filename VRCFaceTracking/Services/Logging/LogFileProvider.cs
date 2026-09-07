using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Services.Logging;

[ProviderAlias("Debug")]
public class LogFileProvider : ILoggerProvider
{
    private readonly StreamWriter? _writer;
    
    public LogFileProvider()
    {
        try
        {
            if (!Directory.Exists(Core.Utils.UserAccessibleDataDirectory)) // Eat my ass windows
                Directory.CreateDirectory(Core.Utils.UserAccessibleDataDirectory);

            var logPath = Path.Combine(Core.Utils.UserAccessibleDataDirectory, "latest.log");

            var file = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096,
                FileOptions.WriteThrough);
            _writer = new StreamWriter(file);
        }
        catch
        {

        }
    }
    
    private readonly ConcurrentDictionary<string, LogFileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName)
    {
        if (_writer != null)
        {
            return _loggers.GetOrAdd(categoryName, name => new LogFileLogger(name, _writer));
        }

        return NullLogger.Instance;
    }

    public void Dispose() => _loggers.Clear();
}