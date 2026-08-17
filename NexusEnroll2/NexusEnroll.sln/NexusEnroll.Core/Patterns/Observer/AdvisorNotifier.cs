using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Observer;

public class AdvisorNotifier : NotificationObserver
{
    public void Update(EnrollmentEvent ev) =>
        Console.WriteLine($"Advisor Notified: Student {ev.Student.Name} has {ev.EventType} Course {ev.Course.Name}");
}
