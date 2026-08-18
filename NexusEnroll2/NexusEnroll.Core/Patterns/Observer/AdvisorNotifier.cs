using NexusEnroll.Core.Interfaces;
using NexusEnroll.Core.Patterns.Factory;

namespace NexusEnroll.Core.Patterns.Observer;

public class AdvisorNotifier : NotificationObserver
{
    private readonly NotificationFactory _factory;

    public AdvisorNotifier(NotificationFactory factory)
    {
        _factory = factory;
    }

    public void Update(EnrollmentEvent ev)
    {
        Notification notification = _factory.CreateNotification(
            "Email",
            "advisor@university.edu",
            $"Advisee {ev.Student.Name} has {ev.EventType} course {ev.Course.Name}.");

        notification.Send();
    }
}