using System.Text.Json;
using System.Text.Json.Serialization;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Serialize responses in camelCase so the SPA can read e.g. `studentId` directly,
// and ignore reference cycles so nested Student <-> Enrollment <-> Course graphs
// (shared by the seeded in-memory repositories) serialize without crashing.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// CORS: open policy so the SPA can be opened from any origin during development.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ---------------------------------------------------------------------------
// DATA TIER
// Repositories are built and fully seeded up-front so cross-references
// (prerequisites, schedules, enrollments, instructors) are wired before DI.
// The same instances are then registered as singletons, so every request,
// service and the front-end share one live in-memory store.
// ---------------------------------------------------------------------------
var studentRepo = new InMemoryStudentRepository();
var courseRepo = new InMemoryCourseRepository();
var adminRepo = new InMemoryAdminRepository();

var faculty = new Faculty("F001", "Dr. Perera");
var adminUser = new Administrator("ADM01", "Admin User");
adminRepo.AddAdmin(adminUser);

// --- Schedules -------------------------------------------------------------
// CS201 and CS301 intentionally share the exact same TimeSlot string so the
// TimeConflictCheckRule strategy has a real conflict to detect.
var monSlot = new Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0), "CS-101")
{
    TimeSlot = "Mon 9:00-11:00"
};

var wedSlot = new Schedule(DayOfWeek.Wednesday, new TimeOnly(10, 0), new TimeOnly(12, 0), "CS-201")
{
    TimeSlot = "Wed 10:00-12:00"
};

var wedSlotConflict = new Schedule(DayOfWeek.Wednesday, new TimeOnly(10, 0), new TimeOnly(12, 0), "CS-301")
{
    TimeSlot = "Wed 10:00-12:00"
};

var friSlot = new Schedule(DayOfWeek.Friday, new TimeOnly(13, 0), new TimeOnly(15, 0), "MGMT-101")
{
    TimeSlot = "Fri 1:00-3:00"
};

var tueSlot = new Schedule(DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(11, 0), "MATH-101")
{
    TimeSlot = "Tue 9:00-11:00"
};

// --- Courses (CS, Math and Business departments) ----------------------------
var cs101 = new Course("CS101", "Intro to Computing", "Fundamentals of programming and computer science.", capacity: 30)
{
    Credits = 3,
    Schedule = monSlot,
    Instructor = faculty,
    EnrolledCount = 10
};

var cs201 = new Course("CS201", "Data Structures", "Stacks, queues, trees and algorithmic analysis.", capacity: 2)
{
    Credits = 3,
    Schedule = wedSlot,
    Instructor = faculty,
    Prerequisites = new List<Course> { cs101 },
    EnrolledCount = 2
};

var cs301 = new Course("CS301", "Algorithms", "Advanced algorithms, complexity and design paradigms.", capacity: 20)
{
    Credits = 3,
    Schedule = wedSlotConflict,
    Instructor = faculty,
    Prerequisites = new List<Course> { cs201 },
    EnrolledCount = 5
};

var mgmt101 = new Course("MGMT101", "Business Management", "Principles of management and business operations.", capacity: 10)
{
    Credits = 3,
    Schedule = friSlot,
    EnrolledCount = 9
};

var math101 = new Course("MATH101", "Calculus I", "Differential and integral calculus foundations.", capacity: 25)
{
    Credits = 3,
    Schedule = tueSlot,
    EnrolledCount = 3
};

faculty.CoursesTaught.AddRange(new[] { cs101, cs201, cs301 });

courseRepo.AddCourse(cs101);
courseRepo.AddCourse(cs201);
courseRepo.AddCourse(cs301);
courseRepo.AddCourse(mgmt101);
courseRepo.AddCourse(math101);

// --- Students ---------------------------------------------------------------
// S001 has completed CS101 (prereq for CS201) AND CS201 so the time-conflict
// scenario against CS301 is reachable from the SPA.
var s001 = new Student("S001", "Devin Gamage", "devin.gamage@university.edu");
s001.CompletedCourses.Add(cs101);
s001.CompletedCourses.Add(cs201);

var s002 = new Student("S002", "Kasun Perera", "kasun.perera@university.edu");
var s003 = new Student("S003", "Amara Silva", "amara.silva@university.edu");

studentRepo.AddStudent(s001);
studentRepo.AddStudent(s002);
studentRepo.AddStudent(s003);

// Background students fill the real seat counts so rosters, seats and the
// capacity report reflect meaningful occupancy.
string[] backgroundIds =
{
    "S010", "S011", "S012", "S013", "S014", "S015",
    "S016", "S017", "S018", "S019", "S020", "S021",
    "S022", "S023", "S024", "S025", "S026", "S027"
};

string[] backgroundNames =
{
    "Nimal Fernando", "Kaveesha Jayasuriya", "Thisara Weerasinghe", "Dinusha Rathnayake",
    "Sachini Dissanayake", "Lasith Wickramasinghe", "Ishara Gunasekara", "Ruwan Perera",
    "Madhavi Silva", "Tharindu Bandara", "Hiruni Jayawardena", "Kavindu Senanayake",
    "Nadeesha Herath", "Chamodi Ekanayake", "Dilan Fernando", "Amaya Kulasekara",
    "Pasindu Liyanage", "Nelum Alwis"
};

var backgroundStudents = new List<Student>();
for (int i = 0; i < backgroundIds.Length; i++)
{
    var s = new Student(backgroundIds[i], backgroundNames[i], $"{backgroundIds[i].ToLowerInvariant()}@university.edu");
    backgroundStudents.Add(s);
    studentRepo.AddStudent(s);
}

static void EnrollSeed(Student student, Course course)
{
    student.EnrolledCourses.Add(
        new Enrollment(Guid.NewGuid().ToString(), student, course, "Enrolled", DateTime.UtcNow.AddDays(-14)));
    course.EnrolledStudents.Add(student);
}

// CS101  : 10 enrolled  -> S010..S019
foreach (Student s in backgroundStudents.Take(10))
{
    EnrollSeed(s, cs101);
}

// CS201  : 2 enrolled (FULL) -> S010, S011 (both have completed CS101)
EnrollSeed(backgroundStudents[0], cs201);
EnrollSeed(backgroundStudents[1], cs201);

// CS301  : 5 enrolled -> S012..S016 (completed CS201, satisfying the prereq)
foreach (Student s in backgroundStudents.Skip(2).Take(5))
{
    s.CompletedCourses.Add(cs201);
    EnrollSeed(s, cs301);
}

// MGMT101: 9 enrolled (90%) -> S003 + S020..S027
EnrollSeed(s003, mgmt101);
foreach (Student s in backgroundStudents.Skip(10))
{
    EnrollSeed(s, mgmt101);
}

// MATH101: 3 enrolled -> S010, S011, S020
EnrollSeed(backgroundStudents[0], math101);
EnrollSeed(backgroundStudents[1], math101);
EnrollSeed(backgroundStudents[10], math101);

// ---------------------------------------------------------------------------
// DI REGISTRATIONS (all singletons - shared in-memory state)
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<IStudentRepository>(studentRepo);
builder.Services.AddSingleton<ICourseRepository>(courseRepo);
builder.Services.AddSingleton<IAdminRepository>(adminRepo);
builder.Services.AddSingleton(faculty);

// Observer pattern: publisher + subscribers (Advisor & Waitlist notifiers)
// plus a NotificationEventLog that captures every event for the API.
builder.Services.AddSingleton<NotificationFactory>(new EmailNotificationFactory());
builder.Services.AddSingleton<EnrollmentEventPublisher>();
builder.Services.AddSingleton<AdvisorNotifier>();
builder.Services.AddSingleton<WaitlistNotifier>();
builder.Services.AddSingleton<NotificationEventLog>();

// Strategy pattern: pluggable validation rules.
builder.Services.AddSingleton<CapacityCheckRule>();
builder.Services.AddSingleton<PrerequisiteCheckRule>();
builder.Services.AddSingleton<TimeConflictCheckRule>();
builder.Services.AddSingleton<EnrollmentValidationRule>(sp => sp.GetRequiredService<CapacityCheckRule>());
builder.Services.AddSingleton<EnrollmentValidationRule>(sp => sp.GetRequiredService<PrerequisiteCheckRule>());
builder.Services.AddSingleton<EnrollmentValidationRule>(sp => sp.GetRequiredService<TimeConflictCheckRule>());

builder.Services.AddSingleton(sp => new EnrollmentService(
    new List<EnrollmentValidationRule>
    {
        sp.GetRequiredService<CapacityCheckRule>(),
        sp.GetRequiredService<PrerequisiteCheckRule>(),
        sp.GetRequiredService<TimeConflictCheckRule>()
    },
    sp.GetRequiredService<EnrollmentEventPublisher>()));

builder.Services.AddSingleton<EnrollmentFacade>();

// Application services.
builder.Services.AddSingleton<StudentService>();
builder.Services.AddSingleton<CatalogueService>();
builder.Services.AddSingleton<FacultyService>();
builder.Services.AddSingleton<ReportingService>();
builder.Services.AddSingleton<AdminService>();

// State pattern: grade submission lifecycle (Pending -> Submitted).
builder.Services.AddSingleton<GradeSubmissionRegistry>();

var app = builder.Build();

// Attach the Observer subscribers to the publisher. The NotificationEventLog
// mirrors AdvisorNotifier (fires on every event) and WaitlistNotifier (fires
// only on "Dropped") so the /api/notifications feed is populated.
var eventPublisher = app.Services.GetRequiredService<EnrollmentEventPublisher>();
eventPublisher.Subscribe(app.Services.GetRequiredService<AdvisorNotifier>());
eventPublisher.Subscribe(app.Services.GetRequiredService<WaitlistNotifier>());
eventPublisher.Subscribe(app.Services.GetRequiredService<NotificationEventLog>());

app.UseCors("AllowAll");

// Serve the SPA from wwwroot/index.html at the application root.
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------------------------------
// ENDPOINTS
// ---------------------------------------------------------------------------

static object CourseDto(Course c) => new
{
    c.CourseId,
    c.Name,
    c.Credits,
    c.Capacity,
    c.EnrolledCount,
    AvailableSeats = c.Capacity - c.EnrolledCount,
    Instructor = c.Instructor?.Name,
    Schedule = c.Schedule is null
        ? null
        : new
        {
            Day = c.Schedule.Day.ToString(),
            Start = c.Schedule.StartTime.ToString(),
            End = c.Schedule.EndTime.ToString(),
            c.Schedule.TimeSlot,
            c.Schedule.Room
        },
    Prerequisites = c.Prerequisites.Select(p => p.CourseId).ToArray()
};

// GET /api/catalogue : real-time seats, schedule and prerequisites.
app.MapGet("/api/catalogue", (ICourseRepository courses) =>
    Results.Ok(courses.GetAllCourses().Select(CourseDto)));

// GET /api/students/{id}/details : profile, enrolled courses, schedule,
// completed courses and academic progress.
app.MapGet("/api/students/{id}/details", (string id, IStudentRepository students) =>
{
    Student? student = students.GetStudent(id);
    if (student is null)
    {
        return Results.NotFound(new { success = false, message = $"Student {id} not found." });
    }

    ProgressReport report = student.ViewProgress();

    return Results.Ok(new
    {
        studentId = student.StudentId,
        name = student.Name,
        email = student.Email,
        completedCourses = student.CompletedCourses.Select(c => new { courseId = c.CourseId, name = c.Name, credits = c.Credits }),
        progress = new { completed = report.CoursesCompleted, totalCredits = report.TotalCompletedCredits },
        schedule = student.EnrolledCourses
            .Where(e => e.Status == "Enrolled")
            .Select(e => new
            {
                courseId = e.Course.CourseId,
                name = e.Course.Name,
                day = e.Course.Schedule?.Day.ToString(),
                start = e.Course.Schedule?.StartTime.ToString(),
                end = e.Course.Schedule?.EndTime.ToString(),
                timeSlot = e.Course.Schedule?.TimeSlot,
                room = e.Course.Schedule?.Room
            })
    });
});

// POST /api/enroll : Strategy-validated enrollment with detailed rule failures.
app.MapPost("/api/enroll", (EnrollRequest req, EnrollmentFacade facade,
    IEnumerable<EnrollmentValidationRule> rules,
    IStudentRepository students, ICourseRepository courses) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(req.StudentId) || string.IsNullOrWhiteSpace(req.CourseId))
        {
            return Results.BadRequest(new { success = false, error = "Student ID and Course ID are required." });
        }

        Student? student = students.GetStudent(req.StudentId);
        Course? course = courses.GetCourse(req.CourseId);
        if (student is null || course is null)
        {
            return Results.BadRequest(new { success = false, error = "Unknown student or course." });
        }

        foreach (EnrollmentValidationRule rule in rules)
        {
            if (!rule.Validate(student, course))
            {
                string reason = rule.GetType().Name switch
                {
                    nameof(CapacityCheckRule) => "Capacity full",
                    nameof(PrerequisiteCheckRule) => "Prerequisite missing",
                    nameof(TimeConflictCheckRule) => "Time conflict",
                    _ => "Validation rule rejected"
                };

                return Results.BadRequest(new
                {
                    success = false,
                    error = $"{reason}: {rule.GetType().Name} rejected enrollment in {course.CourseId} {course.Name} (Strategy pattern)."
                });
            }
        }

        bool ok = facade.Enroll(req.StudentId, req.CourseId);
        return ok
            ? Results.Ok(new { success = true, message = $"{student.Name} enrolled in {course.CourseId} {course.Name}." })
            : Results.BadRequest(new { success = false, error = "Enrollment failed due to an unexpected transaction error." });
    }
    catch (Exception ex)
    {
        // Safety net: surface any thrown Strategy/transaction failure cleanly.
        return Results.BadRequest(new { success = false, error = ex.Message });
    }
});

// POST /api/drop : drops a course and triggers Observer waitlist notifications.
app.MapPost("/api/drop", (DropRequest req, EnrollmentFacade facade,
    IStudentRepository students, ICourseRepository courses) =>
{
    if (string.IsNullOrWhiteSpace(req.StudentId) || string.IsNullOrWhiteSpace(req.CourseId))
    {
        return Results.BadRequest(new { success = false, message = "Student ID and Course ID are required." });
    }

    Student? student = students.GetStudent(req.StudentId);
    Course? course = courses.GetCourse(req.CourseId);
    if (student is null || course is null)
    {
        return Results.BadRequest(new { success = false, message = "Unknown student or course." });
    }

    if (!student.EnrolledCourses.Any(e => e.Course == course))
    {
        return Results.BadRequest(new { success = false, message = $"{student.Name} is not enrolled in {course.CourseId}." });
    }

    bool ok = facade.DropCourse(req.StudentId, req.CourseId);
    return ok
        ? Results.Ok(new { success = true, message = $"Dropped {course.CourseId} {course.Name}. Advisor & waitlist observers notified." })
        : Results.BadRequest(new { success = false, message = "Drop failed." });
});

// GET /api/faculty/{facultyId}/roster/{courseId} : live class roster.
app.MapGet("/api/faculty/{facultyId}/roster/{courseId}", (string facultyId, string courseId,
    Faculty faculty, ICourseRepository courses, GradeSubmissionRegistry grades) =>
{
    if (faculty.FacultyId != facultyId)
    {
        return Results.NotFound(new { success = false, message = $"Faculty {facultyId} not found." });
    }

    Course? course = courses.GetCourse(courseId);
    if (course is null)
    {
        return Results.NotFound(new { success = false, message = $"Course {courseId} not found." });
    }

    var roster = course.EnrolledStudents.Select(s => new
    {
        studentId = s.StudentId,
        name = s.Name,
        email = s.Email,
        gradeStatus = grades.GetStateName(courseId, s.StudentId) ?? "PendingState"
    });

    return Results.Ok(new
    {
        courseId = course.CourseId,
        courseName = course.Name,
        enrolledCount = course.EnrolledCount,
        capacity = course.Capacity,
        roster
    });
});

// POST /api/faculty/grades/submit : State-pattern grade lifecycle.
app.MapPost("/api/faculty/grades/submit", (GradeSubmitRequest req,
    ICourseRepository courses, IStudentRepository students, GradeSubmissionRegistry grades) =>
{
    if (string.IsNullOrWhiteSpace(req.CourseId) || string.IsNullOrWhiteSpace(req.StudentId) || string.IsNullOrWhiteSpace(req.Grade))
    {
        return Results.BadRequest(new { success = false, message = "Course ID, Student ID and Grade are required." });
    }

    Course? course = courses.GetCourse(req.CourseId);
    Student? student = students.GetStudent(req.StudentId);
    if (course is null || student is null)
    {
        return Results.BadRequest(new { success = false, message = "Unknown course or student." });
    }

    if (!course.EnrolledStudents.Contains(student))
    {
        return Results.BadRequest(new { success = false, message = $"{student.Name} is not enrolled in {course.CourseId}." });
    }

    GradeSubmission submission = grades.GetOrCreate(req.CourseId, req.StudentId, req.Grade.ToUpperInvariant());
    string fromState = submission.State.GetType().Name;
    submission.Submit();
    string toState = submission.State.GetType().Name;

    string transition = fromState == toState
        ? $"Grade is already in {toState} - no further transition possible."
        : $"Grade {submission.Grade} for {student.Name} in {course.CourseId} transitioned {fromState} -> {toState} (State pattern).";

    return Results.Ok(new
    {
        success = true,
        message = transition,
        fromState,
        toState
    });
});

// POST /api/admin/override : administrative force-enrollment (bypasses rules).
app.MapPost("/api/admin/override", (OverrideRequest req, EnrollmentFacade facade,
    IAdminRepository admins, IStudentRepository students, ICourseRepository courses) =>
{
    if (string.IsNullOrWhiteSpace(req.AdminId) || string.IsNullOrWhiteSpace(req.StudentId) || string.IsNullOrWhiteSpace(req.CourseId))
    {
        return Results.BadRequest(new { success = false, message = "Admin ID, Student ID and Course ID are required." });
    }

    Administrator? admin = admins.GetAdmin(req.AdminId);
    Student? student = students.GetStudent(req.StudentId);
    Course? course = courses.GetCourse(req.CourseId);
    if (admin is null || student is null || course is null)
    {
        return Results.BadRequest(new { success = false, message = "Unknown admin, student or course." });
    }

    bool ok = facade.AdminOverrideEnrollment(req.AdminId, req.StudentId, req.CourseId);
    return ok
        ? Results.Ok(new { success = true, message = $"{admin.Name} force-enrolled {student.Name} into {course.CourseId} {course.Name} (validation rules bypassed)." })
        : Results.BadRequest(new { success = false, message = "Administrative override failed." });
});

// GET /api/admin/reports/capacity : capacity analytics with >90% alerts.
app.MapGet("/api/admin/reports/capacity", (ICourseRepository courses) =>
{
    var report = courses.GetAllCourses().Select(c =>
    {
        double occupancy = c.Capacity > 0 ? (double)c.EnrolledCount / c.Capacity * 100 : 0;
        return new
        {
            c.CourseId,
            c.Name,
            c.Capacity,
            c.EnrolledCount,
            Available = c.Capacity - c.EnrolledCount,
            OccupancyPercent = Math.Round(occupancy, 1),
            Status = occupancy >= 90 ? "Alert: at or above 90% capacity" : (occupancy >= 75 ? "Warning" : "OK"),
            IsOver90Percent = occupancy >= 90,
            OverCapacity = c.EnrolledCount > c.Capacity
        };
    }).OrderByDescending(r => r.OccupancyPercent);

    return Results.Ok(new { generatedAt = DateTime.Now, courses = report });
});

// GET /api/notifications : recent Observer-pattern event log entries.
app.MapGet("/api/notifications", (NotificationEventLog log) =>
{
    var entries = log.Entries
        .OrderByDescending(e => e.Timestamp)
        .Take(100)
        .Select(e => new { timestamp = e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), e.Message });

    return Results.Ok(entries);
});

app.Run();

// ---------------------------------------------------------------------------
// REQUEST / RESPONSE TYPES
// ---------------------------------------------------------------------------
record EnrollRequest(string? StudentId, string? CourseId);
record DropRequest(string? StudentId, string? CourseId);
record GradeSubmitRequest(string? CourseId, string? StudentId, string? Grade);
record OverrideRequest(string? AdminId, string? StudentId, string? CourseId);

// Observer subscriber that captures every published enrollment event into an
// in-memory log so /api/notifications can expose it to the SPA. It mirrors the
// AdvisorNotifier (all events) and WaitlistNotifier (only "Dropped") behavior.
public class NotificationEventLog : NotificationObserver
{
    private readonly object _gate = new();
    public List<NotificationEntry> Entries { get; } = new();

    public void Update(EnrollmentEvent ev)
    {
        lock (_gate)
        {
            Entries.Add(new NotificationEntry(
                DateTime.Now,
                $"[Observer] Advisor notified for {ev.Student.StudentId} {ev.EventType} in {ev.Course.CourseId} {ev.Course.Name}"));

            if (ev.EventType == "Dropped")
            {
                Entries.Add(new NotificationEntry(
                    DateTime.Now,
                    $"[Observer] Waitlist alerted for {ev.Course.CourseId} {ev.Course.Name}"));
            }
        }
    }
}

public record NotificationEntry(DateTime Timestamp, string Message);

// State-pattern grade registry: one GradeSubmission per (course, student).
public class GradeSubmissionRegistry
{
    private readonly Dictionary<string, GradeSubmission> _submissions = new();
    private readonly object _gate = new();

    private static string Key(string courseId, string studentId) => $"{courseId}|{studentId}";

    public GradeSubmission GetOrCreate(string courseId, string studentId, string grade)
    {
        lock (_gate)
        {
            string key = Key(courseId, studentId);
            if (!_submissions.TryGetValue(key, out GradeSubmission? submission))
            {
                submission = new GradeSubmission(grade);
                _submissions[key] = submission;
            }

            return submission;
        }
    }

    public string? GetStateName(string courseId, string studentId)
    {
        lock (_gate)
        {
            return _submissions.TryGetValue(Key(courseId, studentId), out GradeSubmission? submission)
                ? submission.State.GetType().Name
                : null;
        }
    }
}