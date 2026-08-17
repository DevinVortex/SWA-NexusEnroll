using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Observer;

public class WaitlistNotifier : NotificationObserver
{
    public void Update(EnrollmentEvent ev)
    {
        if (ev.EventType == "Dropped")
        {
            Console.WriteLine($"Waitlist Alert: A seat opened in {ev.Course.Name}. Notifying waitlisted students.");
        }
    }
}
