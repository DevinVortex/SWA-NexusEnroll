using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.State;

public class PendingState : GradeState
{
    public void Submit(GradeSubmission g)
    {
        Console.WriteLine("Grade submitted.");
        g.SetState(new SubmittedState());
    }

    public void Approve(GradeSubmission g) =>
        Console.WriteLine("Cannot approve: grade has not been submitted yet.");

    public void Reject(GradeSubmission g, string reason) =>
        Console.WriteLine("Cannot reject: grade has not been submitted yet.");
}
