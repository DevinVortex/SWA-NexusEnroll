using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Interfaces;

public interface EnrollmentValidationRule
{
    bool Validate(Student student, Course course);
}
