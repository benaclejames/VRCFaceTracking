namespace VRCFaceTracking.Contracts.Services;

public enum NotificationSeverity
{
    Information,
    Success,
    Warning,
    Error
}

/// <summary>
/// Surfaces a short message to the user outside of the normal page flow.
/// Desktop shows an in-window toast; Android posts a system notification so it is visible while the app is backgrounded.
/// Safe to call from any thread.
/// </summary>
public interface INotificationService
{
    void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Information);
}
