using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.Strategy;

public class CapacityCheckRule : EnrollmentValidationRule
{
    public bool Validate(Student student, Course course) => course.HasAvailableSeat();
}
