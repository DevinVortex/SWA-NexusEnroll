using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Patterns.Observer;

public class EnrollmentEvent
{
    public Student Student { get; }
    public Course Course { get; }
    public string EventType { get; }

    public EnrollmentEvent(Student student, Course course, string eventType)
    {
        Student = student;
        Course = course;
        EventType = eventType;
    }
}
