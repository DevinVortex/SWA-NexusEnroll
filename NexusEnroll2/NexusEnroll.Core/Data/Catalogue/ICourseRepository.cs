using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Data.Catalogue;

public interface ICourseRepository
{
    Course GetCourse(string courseId);
    void AddCourse(Course course);
    void UpdateCourse(Course course);
    List<Course> GetAllCourses();
}