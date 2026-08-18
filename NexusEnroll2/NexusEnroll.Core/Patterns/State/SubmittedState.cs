using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.State;

public class SubmittedState : GradeState
{
    public void Submit(GradeSubmission g) =>
        Console.WriteLine("Grade is already submitted.");

    public void Approve(GradeSubmission g) =>
        Console.WriteLine("Grade approved.");

    public void Reject(GradeSubmission g, string reason)
    {
        Console.WriteLine($"Grade rejected: {reason}");
        g.SetState(new RejectedState());
    }
}
