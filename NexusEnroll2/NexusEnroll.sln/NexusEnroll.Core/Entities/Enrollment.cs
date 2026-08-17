namespace NexusEnroll.Core.Entities;

public class Enrollment
{
    public string EnrollmentId { get; }
    public Student Student { get; }
    public Course Course { get; }
    public string Status { get; private set; }
    public DateTime EnrollmentDate { get; }

    public Enrollment(string enrollmentId, Student student, Course course, string status, DateTime enrollmentDate)
    {
        EnrollmentId = enrollmentId;
        Student = student;
        Course = course;
        Status = status;
        EnrollmentDate = enrollmentDate;
    }

    public string GetStatus() => Status;

    public void UpdateStatus(string status) => Status = status;
}
