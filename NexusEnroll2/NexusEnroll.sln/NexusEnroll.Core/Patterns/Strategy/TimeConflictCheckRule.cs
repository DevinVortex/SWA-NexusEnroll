using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Strategy;

public class TimeConflictCheckRule : EnrollmentValidationRule
{
    public bool Validate(Student student, Course course)
    {
        // TODO: Implement schedule overlap logic
        return true;
    }
}
