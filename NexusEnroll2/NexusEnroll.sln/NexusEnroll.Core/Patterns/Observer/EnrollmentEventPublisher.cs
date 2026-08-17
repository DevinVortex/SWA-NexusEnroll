using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Observer;

public class EnrollmentEventPublisher
{
    private readonly List<NotificationObserver> _observers = new();

    public void Subscribe(NotificationObserver observer) => _observers.Add(observer);

    public void Unsubscribe(NotificationObserver observer) => _observers.Remove(observer);

    public void Publish(EnrollmentEvent ev)
    {
        foreach (NotificationObserver observer in _observers)
        {
            observer.Update(ev);
        }
    }
}
