using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Data.Catalogue;

public class InMemoryCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses = new();

    public Course GetCourse(string courseId) =>
        _courses.FirstOrDefault(c => c.CourseId == courseId)!;

    public void AddCourse(Course course) =>
        _courses.Add(course);

    public void UpdateCourse(Course course)
    {
        int index = _courses.FindIndex(c => c.CourseId == course.CourseId);

        if (index >= 0)
        {
            _courses[index] = course;
        }
    }

    public List<Course> GetAllCourses() =>
        _courses;
}