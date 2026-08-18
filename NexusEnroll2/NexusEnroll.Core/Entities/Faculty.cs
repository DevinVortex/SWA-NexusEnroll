using NexusEnroll.Core.Patterns.State;

namespace NexusEnroll.Core.Entities;

public class Faculty
{
    public string FacultyId { get; }
    public string Name { get; }
    public List<Course> CoursesTaught { get; init; } = new();

    public Faculty(string facultyId, string name)
    {
        FacultyId = facultyId;
        Name = name;
    }

    public List<Student> ViewRoster(Course course) => course.EnrolledStudents;

    public List<GradeSubmission> SubmitGrades(Course course, Dictionary<Student, string> studentGrades)
    {
        List<GradeSubmission> submissions = new();

        foreach (KeyValuePair<Student, string> entry in studentGrades)
        {
            GradeSubmission submission = new(entry.Value);
            submission.Submit();
            submissions.Add(submission);
        }

        return submissions;
    }

    public void RequestCourseChange(Course course, string requestedChange) =>
        Console.WriteLine($"Faculty {Name} requested change for {course.Name}: {requestedChange}");
}