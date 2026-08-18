using NexusEnroll.Core.Data.Admin;
using NexusEnroll.Core.Data.Catalogue;
using NexusEnroll.Core.Data.Student;
using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;
using NexusEnroll.Core.Patterns.Facade;
using NexusEnroll.Core.Patterns.Factory;
using NexusEnroll.Core.Patterns.Observer;
using NexusEnroll.Core.Patterns.State;
using NexusEnroll.Core.Patterns.Strategy;
using NexusEnroll.Core.Services;

namespace NexusEnroll.ConsoleApp;

internal static class Program
{
    private static void Main()
    {
        /*
         * SETUP PHASE
         * Compose the entire system using dependency injection-style wiring.
         */
        Console.WriteLine("                  SETUP PHASE                  ");
        Console.WriteLine();

        // --- Factory Method Pattern: build a notification product ---
        NotificationFactory notificationFactory = new EmailNotificationFactory();
        Notification welcomeEmail = notificationFactory.CreateNotification(
            "Email",
            "student.a@university.edu",
            "Welcome to NexusEnroll!");

        Console.WriteLine(">> Sending welcome notification via the Factory...");
        welcomeEmail.Send();
        Console.WriteLine();

        // --- Observer Pattern: create the publisher and attach subscribers ---
        var publisher = new EnrollmentEventPublisher();
        publisher.Subscribe(new AdvisorNotifier(notificationFactory));
        publisher.Subscribe(new WaitlistNotifier(notificationFactory));
        Console.WriteLine(">> Event publisher created; AdvisorNotifier and WaitlistNotifier subscribed.");
        Console.WriteLine();

        // --- Strategy Pattern: pluggable enrollment validation rules ---
        List<EnrollmentValidationRule> validationRules = new()
        {
            new CapacityCheckRule(),      // seats available?
            new PrerequisiteCheckRule(),  // prerequisites completed?
            new TimeConflictCheckRule()   // schedule conflicts?
        };

        // --- Core services (the engine + the facade) ---
        var enrollmentService = new EnrollmentService(validationRules, publisher);

        // Data-tier repositories simulating the Database-per-Service microservices
        // pattern. The Facade depends on these interfaces via constructor injection,
        // so the mock data added during the Data Mocking phase is visible to it.
        var studentRepository = new InMemoryStudentRepository();
        var courseRepository = new InMemoryCourseRepository();
        var adminRepository = new InMemoryAdminRepository();

        var facade = new EnrollmentFacade(enrollmentService, studentRepository, courseRepository, adminRepository);

        /*
         * DATA MOCKING PHASE
         * In-memory test data only -- no database involved.
         */
        Console.WriteLine();
        Console.WriteLine("                  DATA MOCKING PHASE                  ");
        Console.WriteLine();

        var faculty = new Faculty("FAC-001", "Dr. Smith");

        // Two courses intentionally share the exact same time slot so the
        // TimeConflictCheckRule strategy has real data to work with.
        var advancedArchitecture = new Course(
            "CS-401",
            "Advanced Architecture",
            "Deep dive into modern software design.",
            capacity: 1)
        {
            Instructor = faculty,
            Schedule = new Schedule { TimeSlot = "Mon 9:00 AM", Room = "Room 101" }
        };

        var cloudComputing = new Course(
            "CS-402",
            "Cloud Computing",
            "Distributed systems and cloud infrastructure.",
            capacity: 3)
        {
            Schedule = new Schedule { TimeSlot = "Mon 9:00 AM", Room = "Room 102" }
        };

        faculty.CoursesTaught.Add(advancedArchitecture);

        var studentA = new Student("S-001", "Student A", "student.a@university.edu");
        var studentB = new Student("S-002", "Student B", "student.b@university.edu");

        // Give Student A one completed course so ViewProgress has real data.
        studentA.CompletedCourses.Add(new Course("CS-100", "Intro to Computing", "Introductory course.", capacity: 50) { Credits = 3 });

        // --- Administrator module mock ---
        var adminAlpha = new Administrator("admin-01", "Admin Alpha");

        studentRepository.AddStudent(studentA);
        studentRepository.AddStudent(studentB);
        courseRepository.AddCourse(advancedArchitecture);
        courseRepository.AddCourse(cloudComputing);
        adminRepository.AddAdmin(adminAlpha);

        Console.WriteLine($"Faculty : {faculty.Name} ({faculty.FacultyId})");
        Console.WriteLine($"Course  : {advancedArchitecture.Name} ({advancedArchitecture.CourseId}) - Capacity {advancedArchitecture.Capacity}");
        Console.WriteLine($"Course  : {cloudComputing.Name} ({cloudComputing.CourseId}) - Capacity {cloudComputing.Capacity}");
        Console.WriteLine($"Schedule: both courses share TimeSlot [{advancedArchitecture.Schedule!.TimeSlot}]");
        Console.WriteLine($"Students: {studentA.Name} ({studentA.StudentId}), {studentB.Name} ({studentB.StudentId})");
        Console.WriteLine($"Admin   : {adminAlpha.Name} ({adminAlpha.AdminId})");

        /*
         * STRATEGY & OBSERVER: ENROLLMENT + TIME CONFLICT
         */
        Console.WriteLine();
        Console.WriteLine("          STRATEGY & OBSERVER: ENROLLMENT          ");
        Console.WriteLine();

        // Student A enrolls in Course 1 -> every rule passes and the publisher
        // fans out the "Enrolled" event to all subscribed observers.
        Console.WriteLine($">> {studentA.Name} attempts to enroll in {advancedArchitecture.Name}...");
        bool studentACourse1 = facade.Enroll("S-001", "CS-401");
        Console.WriteLine();
        Console.WriteLine($"Student A enrollment : {(studentACourse1 ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Course enrollment    : {advancedArchitecture.EnrolledCount}/{advancedArchitecture.Capacity} seats taken");

        // Faculty checks the class roster right after a successful enrollment.
        Console.WriteLine();
        Console.WriteLine($">> {faculty.Name} views the roster for {advancedArchitecture.Name}...");
        List<Student> roster = faculty.ViewRoster(advancedArchitecture);
        foreach (Student s in roster)
        {
            Console.WriteLine($"   - {s.Name} is on the class roster.");
        }

        // Student A attempts Course 2 in the SAME time slot -> the
        // TimeConflictCheckRule strategy rejects the overlapping schedule.
        Console.WriteLine();
        Console.WriteLine($">> {studentA.Name} attempts to enroll in {cloudComputing.Name} (same time slot)...");
        bool studentACourse2 = facade.Enroll("S-001", "CS-402");
        Console.WriteLine();
        Console.WriteLine($"Student A enrollment : {(studentACourse2 ? "SUCCESS" : "FAILED")}");
        Console.WriteLine("Student A was rejected by the TimeConflictCheckRule strategy (overlapping schedule slot).");

        /*
         * FACULTY CAPABILITIES
         */
        Console.WriteLine();
        Console.WriteLine("                  FACULTY CAPABILITIES                  ");
        Console.WriteLine();

        // Faculty-initiated administrative request flow.
        faculty.RequestCourseChange(advancedArchitecture, "Increase capacity to 40");
        Console.WriteLine();

        /*
         * STATE PATTERN: GRADING
         * Walk the grade lifecycle: Pending -> Submitted -> Approved.
         */
        Console.WriteLine("                  STATE PATTERN: GRADING                  ");
        Console.WriteLine();

        var mockGrades = new Dictionary<Student, string>
        {
            { studentA, "A-" }
        };

        List<GradeSubmission> submissions = faculty.SubmitGrades(advancedArchitecture, mockGrades);

        Console.WriteLine($">> {faculty.Name} submitted grades for {advancedArchitecture.Name}...");
        Console.WriteLine();

        foreach (GradeSubmission submission in submissions)
        {
            Console.WriteLine($"Grade value          : {submission.Grade}");
            Console.WriteLine($"State after Submit() : {submission.State.GetType().Name}");

            submission.Approve();
            Console.WriteLine($"State after Approve(): {submission.State.GetType().Name}");
        }

        /*
         * STUDENT CAPABILITIES
         */
        Console.WriteLine();
        Console.WriteLine("                  STUDENT CAPABILITIES                  ");
        Console.WriteLine();

        // Student A views their academic progress report.
        Console.WriteLine($">> {studentA.Name} views their academic progress...");
        ProgressReport report = studentA.ViewProgress();
        Console.WriteLine($"Progress report     : {report.CoursesCompleted} completed course(s), {report.TotalCompletedCredits} credit(s).");
        Console.WriteLine();

        // Student A browses the catalogue and checks their current schedule.
        Console.WriteLine($">> {studentA.Name} browses the course catalogue...");
        List<Course> browseResult = studentA.BrowseCatalogue(c => c.Capacity > 0, courseRepository.GetAllCourses());
        foreach (Course c in browseResult)
        {
            Console.WriteLine($"   - {c.Name} ({c.CourseId}) - {c.EnrolledCount}/{c.Capacity} seats taken");
        }

        Console.WriteLine();
        Console.WriteLine($">> {studentA.Name} views their current schedule...");
        List<Course> schedule = studentA.ViewSchedule();
        foreach (Course c in schedule)
        {
            Console.WriteLine($"   - {c.Name} at {c.Schedule?.TimeSlot ?? "TBD"}");
        }

        /*
         * ADMINISTRATOR MODULE
         */
        Console.WriteLine();
        Console.WriteLine("                  ADMINISTRATOR MODULE                  ");
        Console.WriteLine();

        // Standard administrative tools.
        adminAlpha.ManageCourse(advancedArchitecture);
        adminAlpha.ManageAccount(studentA);
        Report adminReport = adminAlpha.GenerateReport("Enrolment Trends");
        Console.WriteLine($"Report              : {adminReport.Title} ({adminReport.ReportType})");
        Console.WriteLine($"Generated on        : {adminReport.GeneratedDate:yyyy-MM-dd HH:mm}");
        Console.WriteLine("Report content:");
        foreach (string row in adminReport.Content)
        {
            Console.WriteLine($"   {row}");
        }
        Console.WriteLine();

        // Student B tries to enroll in the full Course 1 -> the
        // CapacityCheckRule strategy rejects them on the regular path.
        Console.WriteLine($">> {studentB.Name} attempts to enroll in the full {advancedArchitecture.Name}...");
        bool studentBFailed = facade.Enroll("S-002", "CS-401");
        Console.WriteLine($"Student B enrollment : {(studentBFailed ? "SUCCESS" : "FAILED")}");
        Console.WriteLine("Student B was rejected by the CapacityCheckRule strategy.");
        Console.WriteLine();

        // The Administrator uses their override privilege to bypass every rule
        // and force-enroll Student B via the domain entity.
        Console.WriteLine($">> {adminAlpha.Name} overrides the rules and force-enrolls {studentB.Name}...");
        bool adminOverrideResult = facade.AdminOverrideEnrollment("admin-01", "S-002", "CS-401");
        Console.WriteLine($"Admin override      : {(adminOverrideResult ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Course enrollment   : {advancedArchitecture.EnrolledCount}/{advancedArchitecture.Capacity} seats taken (over capacity)");
        Console.WriteLine($"Roster              : {string.Join(", ", advancedArchitecture.EnrolledStudents.Select(s => s.Name))}");
        Console.WriteLine();

        // Facade-level course browsing (open seats only).
        Console.WriteLine(">> Facade.BrowseCourses (courses with open seats)...");
        List<Course> openCourses = facade.BrowseCourses(c => c.HasAvailableSeat());
        foreach (Course c in openCourses)
        {
            Console.WriteLine($"   - {c.Name} ({c.CourseId}) - {c.EnrolledCount}/{c.Capacity} seats taken");
        }

        /*
         * TRANSACTION MANAGEMENT: REGULAR ENROLLMENT ROLLBACK
         * All-or-nothing enrollment: a mid-commit crash must roll back state.
         */
        Console.WriteLine();
        Console.WriteLine("          TRANSACTION MANAGEMENT: ALL-OR-NOTHING          ");
        Console.WriteLine();

        var crashCourse = new Course("CS-500", "Transaction Lab", "Demonstrates atomic enrollment.", capacity: 2);
        courseRepository.AddCourse(crashCourse);

        Console.WriteLine($">> Simulating a database crash while {studentB.Name} enrolls in {crashCourse.Name}...");
        enrollmentService.SimulateFailure = true;

        bool crashResult = facade.Enroll("S-002", "CS-500");
        Console.WriteLine($"Enrollment attempt : {(crashResult ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Rollback check     : {studentB.EnrolledCourses.Count} enrollment(s) on student, " +
                          $"{crashCourse.EnrolledCount}/{crashCourse.Capacity} seats taken, " +
                          $"{crashCourse.EnrolledStudents.Count} student(s) on roster");

        /*
         * TRANSACTION MANAGEMENT: ADMIN OVERRIDE ROLLBACK
         * The ForceEnroll path is equally protected by a compensating rollback.
         */
        Console.WriteLine();
        Console.WriteLine("   TRANSACTION MANAGEMENT: ADMIN OVERRIDE ROLLBACK   ");
        Console.WriteLine();

        Console.WriteLine($">> Simulating a database crash during {adminAlpha.Name}'s force-enroll of {studentA.Name}...");
        bool adminCrashResult = facade.AdminOverrideEnrollment("admin-01", "S-001", "CS-500");
        Console.WriteLine($"Admin override     : {(adminCrashResult ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Rollback check     : {studentA.EnrolledCourses.Count} enrollment(s) on student, " +
                          $"{crashCourse.EnrolledCount}/{crashCourse.Capacity} seats taken, " +
                          $"{crashCourse.EnrolledStudents.Count} student(s) on roster");

        enrollmentService.SimulateFailure = false;

        Console.WriteLine();
        Console.WriteLine("                  END OF DEMO                  ");
    }
}