using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Services;

public class EnrollmentFacade
{
    private readonly EnrollmentService _enrollmentService;
    private readonly List<Student> _students;
    private readonly List<Course> _courses;

    public EnrollmentFacade(EnrollmentService enrollmentService, List<Student> students, List<Course> courses)
    {
        _enrollmentService = enrollmentService;
        _students = students;
        _courses = courses;
    }

    public bool Enroll(string studentId, string courseId)
    {
        Student? student = _students.FirstOrDefault(s => s.StudentId == studentId);
        Course? course = _courses.FirstOrDefault(c => c.CourseId == courseId);

        if (student is null || course is null)
        {
            return false;
        }

        return _enrollmentService.ProcessEnrollment(student, course);
    }

    public bool DropCourse(string studentId, string courseId)
    {
        Student? student = _students.FirstOrDefault(s => s.StudentId == studentId);
        Course? course = _courses.FirstOrDefault(c => c.CourseId == courseId);

        if (student is null || course is null)
        {
            return false;
        }

        _enrollmentService.ProcessDrop(student, course);
        return true;
    }

    public List<Course> BrowseCourses(Func<Course, bool> criteria) =>
        _courses.Where(criteria).ToList();
}
