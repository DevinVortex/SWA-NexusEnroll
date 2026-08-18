using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Factory;

public abstract class NotificationFactory
{
    public abstract Notification CreateNotification(string type, string recipient, string message);
}