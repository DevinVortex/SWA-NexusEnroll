using NexusEnroll.Core.Interfaces;

namespace NexusEnroll.Core.Patterns.State;

public class GradeSubmission
{
    public string Grade { get; }
    public GradeState State { get; private set; }

    public GradeSubmission(string grade)
    {
        Grade = grade;
        State = new PendingState();
    }

    public void Submit() => State.Submit(this);

    public void Approve() => State.Approve(this);

    public void Reject(string reason) => State.Reject(this, reason);

    public void SetState(GradeState state) => State = state;
}
