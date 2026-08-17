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

    public List<Student> ViewRoster(Course course) => new();

    public void SubmitGrades(Course course, IReadOnlyDictionary<string, double> grades)
    {
    }

    public void RequestCourseChange(Course course, string requestedChange)
    {
    }
}
