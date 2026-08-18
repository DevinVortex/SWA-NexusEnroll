using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;
using NexusEnroll.Core.Patterns.Observer;

namespace NexusEnroll.Core.Services;

public class EnrollmentService
{
    private readonly List<EnrollmentValidationRule> _validationRules;
    private readonly EnrollmentEventPublisher _eventPublisher;

    public bool SimulateFailure { get; set; }

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

        Enrollment? enrollment = null;

        try
        {
            enrollment = new Enrollment(
                Guid.NewGuid().ToString(),
                student,
                course,
                "Enrolled",
                DateTime.UtcNow);

            // Step A: add the course to the student's enrolled courses.
            student.EnrolledCourses.Add(enrollment);

            // Step B: increment the course's enrolled count.
            course.EnrolledCount++;

            // Step C: simulate a late transaction failure
            // (e.g., a database constraint violation mid-commit).
            if (SimulateFailure)
            {
                throw new Exception("Simulated Database Transaction Crash");
            }

            course.EnrolledStudents.Add(student);

            _eventPublisher.Publish(new EnrollmentEvent(student, course, "Enrolled"));

            return true;
        }
        catch
        {
            Console.WriteLine("Transaction Failed! Rolling back state changes...");

            // Compensating actions: restore the exact pre-call state.
            if (enrollment is not null)
            {
                student.EnrolledCourses.Remove(enrollment);
            }

            course.EnrolledCount--;
            course.EnrolledStudents.Remove(student);

            return false;
        }
    }

    public bool ForceEnroll(Student student, Course course)
    {
        Enrollment? enrollment = null;

        try
        {
            enrollment = new Enrollment(
                Guid.NewGuid().ToString(),
                student,
                course,
                "Enrolled",
                DateTime.UtcNow);

            // Step A: add the course to the student's enrolled courses.
            student.EnrolledCourses.Add(enrollment);

            // Step B: increment the course's enrolled count.
            course.EnrolledCount++;

            // Step C: simulate a late transaction failure
            // (e.g., a database constraint violation mid-commit).
            if (SimulateFailure)
            {
                throw new Exception("Simulated Admin Transaction Crash");
            }

            course.EnrolledStudents.Add(student);

            _eventPublisher.Publish(new EnrollmentEvent(student, course, "Admin Force-Enrolled"));

            return true;
        }
        catch
        {
            Console.WriteLine("Admin Transaction Failed! Rolling back administrative changes...");

            // Compensating actions: restore the exact pre-call state.
            if (enrollment is not null)
            {
                student.EnrolledCourses.Remove(enrollment);
            }

            course.EnrolledCount--;
            course.EnrolledStudents.Remove(student);

            return false;
        }
    }

    public void ProcessDrop(Student student, Course course)
    {
        Enrollment? enrollment = student.EnrolledCourses.FirstOrDefault(e => e.Course == course);

        if (enrollment is not null)
        {
            student.EnrolledCourses.Remove(enrollment);
            course.EnrolledCount--;
            course.EnrolledStudents.Remove(student);
        }

        _eventPublisher.Publish(new EnrollmentEvent(student, course, "Dropped"));
    }
}