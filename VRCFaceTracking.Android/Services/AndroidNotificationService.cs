using Android.App;
using Android.Content;
using VRCFaceTracking.Contracts.Services;
using AndroidApplication = Android.App.Application;

namespace VRCFaceTracking.Android.Services;

// Posts real system notifications so messages are still seen while VRCFT is in the background (e.g. while in VR).
public class AndroidNotificationService : INotificationService
{
    private const string ChannelId = "vrcft_general";

    private readonly Context _context = AndroidApplication.Context;
    private readonly NotificationManager? _manager;
    private int _nextId;

    public AndroidNotificationService()
    {
        _manager = (NotificationManager?)_context.GetSystemService(Context.NotificationService);
        _manager?.CreateNotificationChannel(new NotificationChannel(ChannelId, "VRCFaceTracking", NotificationImportance.Default));
    }

    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Information)
    {
        // Silently dropped by the OS if the user declined POST_NOTIFICATIONS
        if (_manager == null)
            return;

        var launchIntent = _context.PackageManager?.GetLaunchIntentForPackage(_context.PackageName!);
        var contentIntent = launchIntent == null
            ? null
            : PendingIntent.GetActivity(_context, 0, launchIntent, PendingIntentFlags.Immutable);

        var notification = new Notification.Builder(_context, ChannelId)
            .SetSmallIcon(Resource.Drawable.ic_notification)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetStyle(new Notification.BigTextStyle().BigText(message))
            .SetContentIntent(contentIntent)
            .SetAutoCancel(true)
            .Build();

        _manager.Notify(Interlocked.Increment(ref _nextId), notification);
    }
}
