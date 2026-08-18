using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Services;

namespace NexusEnroll.Core.Patterns.Facade;

public class EnrollmentFacade
{
    private readonly EnrollmentService _enrollmentService;
    private readonly List<Student> _students;
    private readonly List<Course> _courses;
    private readonly List<Administrator> _administrators;

    public EnrollmentFacade(
        EnrollmentService enrollmentService,
        List<Student> students,
        List<Course> courses,
        List<Administrator> administrators)
    {
        _enrollmentService = enrollmentService;
        _students = students;
        _courses = courses;
        _administrators = administrators;
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

    public bool AdminOverrideEnrollment(string adminId, string studentId, string courseId)
    {
        Administrator? admin = _administrators.FirstOrDefault(a => a.AdminId == adminId);
        Student? student = _students.FirstOrDefault(s => s.StudentId == studentId);
        Course? course = _courses.FirstOrDefault(c => c.CourseId == courseId);

        if (admin is null || student is null || course is null)
        {
            return false;
        }

        // Strictly delegate through the domain entity. The Administrator bypasses
        // every validation rule and force-enrolls the student via the service.
        return admin.OverrideEnrollment(student, course, _enrollmentService);
    }

    public List<Course> BrowseCourses(Func<Course, bool> criteria) =>
        _courses.Where(criteria).ToList();
}
