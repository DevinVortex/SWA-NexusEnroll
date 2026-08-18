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

    public List<Course> BrowseCatalogue(Func<Course, bool> criteria, List<Course> fullCatalogue) =>
        fullCatalogue.Where(criteria).ToList();

    public List<Course> ViewSchedule() =>
        EnrolledCourses
            .Where(e => e.Status == "Enrolled")
            .Select(e => e.Course)
            .ToList();

    public ProgressReport ViewProgress()
    {
        var report = new ProgressReport
        {
            CoursesCompleted = CompletedCourses.Count,
            TotalCompletedCredits = CompletedCourses.Sum(c => c.Credits)
        };

        Console.WriteLine($"Student {Name} has completed {report.CoursesCompleted} course(s) totaling {report.TotalCompletedCredits} credits.");

        return report;
    }
}
