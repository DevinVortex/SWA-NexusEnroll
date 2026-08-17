using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.State;

public class RejectedState : GradeState
{
    public void Submit(GradeSubmission g)
    {
        Console.WriteLine("Resubmitting grade...");
        g.SetState(new SubmittedState());
    }

    public void Approve(GradeSubmission g) =>
        Console.WriteLine("Cannot approve: grade has been rejected.");

    public void Reject(GradeSubmission g, string reason) =>
        Console.WriteLine("Grade is already rejected.");
}
