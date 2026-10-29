using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;

namespace VRCFaceTracking.Services;

// Default for platforms that haven't provided a native notification surface: the message still lands in the output log.
public class LogNotificationService(ILogger<LogNotificationService> logger) : INotificationService
{
    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Information)
    {
        var level = severity switch
        {
            NotificationSeverity.Warning => LogLevel.Warning,
            NotificationSeverity.Error => LogLevel.Error,
            _ => LogLevel.Information
        };
        logger.Log(level, "{Title}: {Message}", title, message);
    }
}
