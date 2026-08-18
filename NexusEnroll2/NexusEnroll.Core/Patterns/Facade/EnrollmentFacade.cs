using NexusEnroll.Core.Data.Admin;
using NexusEnroll.Core.Data.Catalogue;
using NexusEnroll.Core.Data.Student;
using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Services;

namespace NexusEnroll.Core.Patterns.Facade;

public class EnrollmentFacade
{
    private readonly EnrollmentService _enrollmentService;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IAdminRepository _adminRepository;

    public EnrollmentFacade(
        EnrollmentService enrollmentService,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IAdminRepository adminRepository)
    {
        _enrollmentService = enrollmentService;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _adminRepository = adminRepository;
    }

    public bool Enroll(string studentId, string courseId)
    {
        Student? student = _studentRepository.GetStudent(studentId);
        Course? course = _courseRepository.GetCourse(courseId);

        if (student is null || course is null)
        {
            return false;
        }

        return _enrollmentService.ProcessEnrollment(student, course);
    }

    public bool DropCourse(string studentId, string courseId)
    {
        Student? student = _studentRepository.GetStudent(studentId);
        Course? course = _courseRepository.GetCourse(courseId);

        if (student is null || course is null)
        {
            return false;
        }

        _enrollmentService.ProcessDrop(student, course);
        return true;
    }

    public bool AdminOverrideEnrollment(string adminId, string studentId, string courseId)
    {
        Administrator? admin = _adminRepository.GetAdmin(adminId);
        Student? student = _studentRepository.GetStudent(studentId);
        Course? course = _courseRepository.GetCourse(courseId);

        if (admin is null || student is null || course is null)
        {
            return false;
        }

        // Strictly delegate through the domain entity. The Administrator bypasses
        // every validation rule and force-enrolls the student via the service.
        return admin.OverrideEnrollment(student, course, _enrollmentService);
    }

    public List<Course> BrowseCourses(Func<Course, bool> criteria) =>
        _courseRepository.GetAllCourses().Where(criteria).ToList();
}