using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Factory;

public class EmailNotificationFactory : NotificationFactory
{
    public override Notification CreateNotification(string type, string recipient, string message)
    {
        if (type != "Email")
        {
            throw new ArgumentException($"Unsupported notification type: {type}");
        }

        return new EmailNotification(recipient, message);
    }
}