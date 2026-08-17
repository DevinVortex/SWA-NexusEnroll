using NexusEnroll.Core.Entities;
using NexusEnroll.Core.Interfaces;
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
        publisher.Subscribe(new AdvisorNotifier());
        publisher.Subscribe(new WaitlistNotifier());
        Console.WriteLine(">> Event publisher created; AdvisorNotifier and WaitlistNotifier subscribed.");
        Console.WriteLine();

        // --- Strategy Pattern: pluggable enrollment validation rules ---
        List<EnrollmentValidationRule> validationRules = new()
        {
            new CapacityCheckRule(),      // seats available?
            new PrerequisiteCheckRule(),  // prerequisites completed?
            new TimeConflictCheckRule()   // schedule conflicts? (placeholder)
        };

        // --- Core services (the engine + the facade) ---
        var enrollmentService = new EnrollmentService(validationRules, publisher);

        // Mock data stores. The Facade keeps references to these lists, so the
        // students/courses added during the Data Mocking phase below are visible to it.
        var students = new List<Student>();
        var courses = new List<Course>();

        var facade = new EnrollmentFacade(enrollmentService, students, courses);

        /* 
         * DATA MOCKING PHASE
         * In-memory test data only -- no database involved.
         */
        Console.WriteLine();
        Console.WriteLine("                  DATA MOCKING PHASE                  ");
        Console.WriteLine();

        var faculty = new Faculty("FAC-001", "Dr. Smith");

        var course = new Course(
            "CS-401",
            "Advanced Software Architecture",
            "Deep dive into modern software design.",
            capacity: 1)
        {
            Instructor = faculty
        };

        faculty.CoursesTaught.Add(course);

        var studentA = new Student("S-001", "Student A", "student.a@university.edu");
        var studentB = new Student("S-002", "Student B", "student.b@university.edu");

        students.Add(studentA);
        students.Add(studentB);
        courses.Add(course);

        Console.WriteLine($"Faculty : {faculty.Name} ({faculty.FacultyId})");
        Console.WriteLine($"Course  : {course.Name} ({course.CourseId}) - Capacity {course.Capacity}");
        Console.WriteLine($"Students: {studentA.Name} ({studentA.StudentId}), {studentB.Name} ({studentB.StudentId})");

        /* 
         * STRATEGY & OBSERVER DEMO
         * Student A enrolls (success), Student B is blocked by capacity.
         */
        Console.WriteLine();
        Console.WriteLine("                  STRATEGY & OBSERVER: ENROLLMENT                  ");
        Console.WriteLine();

        // Student A passes every validation rule -> enrollment succeeds and the
        // publisher fans out the "Enrolled" event to all subscribed observers.
        Console.WriteLine($">> {studentA.Name} attempts to enroll in {course.Name}...");
        bool studentAResult = facade.Enroll("S-001", "CS-401");
        Console.WriteLine();
        Console.WriteLine($"Student A enrollment : {(studentAResult ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Course enrollment    : {course.EnrolledCount}/{course.Capacity} seats taken");
        Console.WriteLine();

        // Student B is rejected: the CapacityCheckRule strategy returns false,
        // so ProcessEnrollment short-circuits before any state change or event.
        Console.WriteLine($">> {studentB.Name} attempts to enroll in the same course...");
        bool studentBResult = facade.Enroll("S-002", "CS-401");
        Console.WriteLine();
        Console.WriteLine($"Student B enrollment : {(studentBResult ? "SUCCESS" : "FAILED")}");
        Console.WriteLine("Student B was rejected by the CapacityCheckRule strategy.");

        /* 
         * STATE PATTERN DEMO
         * Walk the grade lifecycle: Pending -> Submitted -> Approved.
         */
        Console.WriteLine();
        Console.WriteLine("                  STATE PATTERN: GRADING                  ");
        Console.WriteLine();

        var gradeSubmission = new GradeSubmission("A-");
        Console.WriteLine($"Grade value          : {gradeSubmission.Grade}");
        Console.WriteLine($"Initial state       : {gradeSubmission.State.GetType().Name}");

        gradeSubmission.Submit();
        Console.WriteLine($"State after Submit() : {gradeSubmission.State.GetType().Name}");

        gradeSubmission.Approve();
        Console.WriteLine($"State after Approve(): {gradeSubmission.State.GetType().Name}");

        Console.WriteLine();
        Console.WriteLine("                  END OF DEMO                  ");
    }
}