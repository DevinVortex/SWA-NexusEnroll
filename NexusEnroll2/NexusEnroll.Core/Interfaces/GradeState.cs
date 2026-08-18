using NexusEnroll.Core.Patterns.State;

namespace NexusEnroll.Core.Interfaces;

public interface GradeState
{
    void Submit(GradeSubmission g);
    void Approve(GradeSubmission g);
    void Reject(GradeSubmission g, string reason);
}
