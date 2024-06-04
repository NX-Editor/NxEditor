using Avalonia.Controls.Notifications;
using Humanizer;
using NxEditor.Core.Models;

namespace NxEditor.Components;

public class ApplicationNotificationManager(WindowNotificationManager windowNotificationManager)
{
    public void ShowSimpleNotification(string message, Severity severity)
    {
        LogEvent logEvent = new(message, severity);
        windowNotificationManager.Show(GetNotification(logEvent));
    }

    public void ShowLogEventNotification(LogEvent logEvent)
    {
        windowNotificationManager.Show(GetNotification(logEvent));
    }

    private static Notification GetNotification(LogEvent logEvent)
    {
        return logEvent.Exception switch {
            Exception ex => new Notification(
                title: $"{logEvent.Exception.GetType().Name.Humanize(LetterCasing.Title)}",
                ex.ToString(),
                (NotificationType)Severity.Error,
                TimeSpan.FromSeconds(5.0)
            ),
            _ => new Notification(
                logEvent.Severity.ToString(),
                logEvent.Message,
                (NotificationType)logEvent.Severity,
                TimeSpan.FromSeconds(2.5)
            )
        };
    }
}
