namespace NexusEnroll.Core.Entities;

public class Course
{
    public string CourseId { get; }
    public string Name { get; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public int EnrolledCount { get; set; }
    public Schedule? Schedule { get; set; }
    public Faculty? Instructor { get; set; }
    public List<Course> Prerequisites { get; init; } = new();
    public List<Student> EnrolledStudents { get; init; } = new();
    public int Credits { get; set; }

    public Course(string courseId, string name, string description, int capacity)
    {
        CourseId = courseId;
        Name = name;
        Description = description;
        Capacity = capacity;
    }

    public bool HasAvailableSeat() => EnrolledCount < Capacity;

    public bool HasPrerequisite(Course course) => Prerequisites.Contains(course);
}
