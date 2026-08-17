using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Strategy;

public class PrerequisiteCheckRule : EnrollmentValidationRule
{
    public bool Validate(Student student, Course course) =>
        course.Prerequisites.All(p => student.CompletedCourses.Contains(p));
}
