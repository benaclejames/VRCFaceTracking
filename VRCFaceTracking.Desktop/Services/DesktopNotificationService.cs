using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;

namespace VRCFaceTracking.Desktop.Services;

// Windows, Linux and macOS have no common OS toast API, so desktop uses an in-window toast which behaves the same everywhere.
public class DesktopNotificationService(ILogger<DesktopNotificationService> logger) : INotificationService
{
    private WindowNotificationManager? _manager;

    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Information)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var manager = GetOrCreateManager();
            if (manager == null)
            {
                // No window yet (or the app is shutting down); don't lose the message.
                logger.LogInformation("{Title}: {Message}", title, message);
                return;
            }

            manager.Show(new Notification(title, message, severity switch
            {
                NotificationSeverity.Success => NotificationType.Success,
                NotificationSeverity.Warning => NotificationType.Warning,
                NotificationSeverity.Error => NotificationType.Error,
                _ => NotificationType.Information
            }));
        });
    }

    private WindowNotificationManager? GetOrCreateManager()
    {
        if (_manager != null)
            return _manager;

        var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var topLevel = TopLevel.GetTopLevel(window);
        if (topLevel == null)
            return null;

        _manager = new WindowNotificationManager(topLevel)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };
        return _manager;
    }
}
