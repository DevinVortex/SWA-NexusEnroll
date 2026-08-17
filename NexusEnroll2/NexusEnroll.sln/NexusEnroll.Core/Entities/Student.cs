namespace NexusEnroll.Core.Entities;

public class Student
{
    public string StudentId { get; }
    public string Name { get; }
    public string Email { get; }
    public List<Enrollment> EnrolledCourses { get; init; } = new();
    public List<Course> CompletedCourses { get; init; } = new();

    public Student(string studentId, string name, string email)
    {
        StudentId = studentId;
        Name = name;
        Email = email;
    }

    public List<Course> BrowseCatalogue(Func<Course, bool> criteria) => new();

    public List<Course> ViewSchedule() =>
        EnrolledCourses
            .Where(e => e.Status == "Enrolled")
            .Select(e => e.Course)
            .ToList();

    public ProgressReport ViewProgress() => new();
}
