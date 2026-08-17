using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;
using NexusEnroll.Core.Patterns.Observer;

namespace NexusEnroll.Core.Services;

public class EnrollmentService
{
    private readonly List<EnrollmentValidationRule> _validationRules;
    private readonly EnrollmentEventPublisher _eventPublisher;

    public EnrollmentService(List<EnrollmentValidationRule> validationRules, EnrollmentEventPublisher eventPublisher)
    {
        _validationRules = validationRules;
        _eventPublisher = eventPublisher;
    }

    public bool ProcessEnrollment(Student student, Course course)
    {
        foreach (EnrollmentValidationRule rule in _validationRules)
        {
            if (!rule.Validate(student, course))
            {
                return false;
            }
        }

        var enrollment = new Enrollment(
            Guid.NewGuid().ToString(),
            student,
            course,
            "Enrolled",
            DateTime.UtcNow);

        student.EnrolledCourses.Add(enrollment);
        course.EnrolledCount++;
        _eventPublisher.Publish(new EnrollmentEvent(student, course, "Enrolled"));

        return true;
    }

    public void ProcessDrop(Student student, Course course)
    {
        Enrollment? enrollment = student.EnrolledCourses.FirstOrDefault(e => e.Course == course);

        if (enrollment is not null)
        {
            student.EnrolledCourses.Remove(enrollment);
            course.EnrolledCount--;
        }

        _eventPublisher.Publish(new EnrollmentEvent(student, course, "Dropped"));
    }
}
