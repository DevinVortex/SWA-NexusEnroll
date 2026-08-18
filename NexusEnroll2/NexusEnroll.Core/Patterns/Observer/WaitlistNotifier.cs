using NexusEnroll.Core.Interfaces;
using NexusEnroll.Core.Patterns.Factory;

namespace NexusEnroll.Core.Patterns.Observer;

public class WaitlistNotifier : NotificationObserver
{
    private readonly NotificationFactory _factory;

    public WaitlistNotifier(NotificationFactory factory)
    {
        _factory = factory;
    }

    public void Update(EnrollmentEvent ev)
    {
        if (ev.EventType == "Dropped")
        {
            Notification notification = _factory.CreateNotification(
                "Email",
                "waitlist@university.edu",
                $"A spot has opened up in {ev.Course.Name} because a student dropped.");

            notification.Send();
        }
    }
}