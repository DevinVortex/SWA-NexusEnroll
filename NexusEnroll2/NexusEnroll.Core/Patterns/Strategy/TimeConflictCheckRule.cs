using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Strategy;

public class TimeConflictCheckRule : EnrollmentValidationRule
{
    public bool Validate(Student student, Course course)
    {
        if (course.Schedule is null)
        {
            return true;
        }

        bool hasConflict = student.EnrolledCourses.Any(e =>
            e.Course.Schedule is not null &&
            e.Course.Schedule.TimeSlot == course.Schedule.TimeSlot);

        return !hasConflict;
    }
}