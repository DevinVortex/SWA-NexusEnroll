using NexusEnroll.Core.Patterns.Observer;

namespace NexusEnroll.Core.Interfaces;

public interface NotificationObserver
{
    void Update(EnrollmentEvent ev);
}
