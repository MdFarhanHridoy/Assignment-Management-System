using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Options;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain;
using AssignmentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AssignmentManagement.UnitTests.TestHelpers;

/// <summary>
/// Centralised factory for creating a fresh EF Core In-Memory database
/// pre-seeded with deterministic test data.  Each call returns an isolated
/// database so tests never interfere with each other.
/// </summary>
public static class TestDbHelper
{
    // ── Fixed GUIDs for predictable test data ──────────────────────────
    public static readonly Guid AdminId         = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid TeacherId        = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    public static readonly Guid Teacher2Id       = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    public static readonly Guid StudentId        = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");
    public static readonly Guid Student2Id       = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005");
    public static readonly Guid Student3Id       = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006");

    public static readonly Guid ClassId           = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid Class2Id          = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    public static readonly Guid SubjectId         = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    public static readonly Guid Subject2Id        = Guid.Parse("cccccccc-0000-0000-0000-000000000002");

    public static readonly Guid TeacherClassSubjectId = Guid.Parse("11111111-0000-0000-0000-000000000001");

    public static readonly Guid EnrollmentId      = Guid.Parse("22222222-0000-0000-0000-000000000001");

    // Assignments
    public static readonly Guid DraftAssignmentId         = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    public static readonly Guid PublishedAssignmentId     = Guid.Parse("dddddddd-0000-0000-0000-000000000002");
    public static readonly Guid PastDeadlineAssignmentId  = Guid.Parse("dddddddd-0000-0000-0000-000000000003");
    public static readonly Guid NoResubmissionAssignmentId = Guid.Parse("dddddddd-0000-0000-0000-000000000004");

    // Submission
    public static readonly Guid SubmissionId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");

    // ── Factory ────────────────────────────────────────────────────────
    public static async Task<AppDbContext> CreateDbAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        var db = new AppDbContext(options);
        await SeedAsync(db);
        return db;
    }

    /// <summary>
    /// Creates an empty in-memory DB (no seed data) for tests that want
    /// to build their own state from scratch.
    /// </summary>
    public static AppDbContext CreateEmptyDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"empty-{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    // ── Seed ───────────────────────────────────────────────────────────
    private static async Task SeedAsync(AppDbContext db)
    {
        // Users
        db.Users.AddRange(
            NewUser(AdminId,    "Admin User",   "admin@example.com",   UserRole.Admin),
            NewUser(TeacherId,   "Teacher One",  "teacher@example.com",  UserRole.Teacher),
            NewUser(Teacher2Id,  "Teacher Two",  "teacher2@example.com", UserRole.Teacher),
            NewUser(StudentId,   "Student One",  "student@example.com",  UserRole.Student),
            NewUser(Student2Id,  "Student Two",  "student2@example.com", UserRole.Student),
            NewUser(Student3Id,  "Student Three","student3@example.com", UserRole.Student)
        );

        // Classes
        db.Classes.AddRange(
            new Class { Id = ClassId,  Name = "Class A", Description = "Test class A" },
            new Class { Id = Class2Id, Name = "Class B", Description = "Test class B" }
        );

        // Subjects (Subject1 in Class1, Subject2 in Class1)
        db.Subjects.AddRange(
            new Subject { Id = SubjectId,  Name = "Math",    ClassId = ClassId },
            new Subject { Id = Subject2Id, Name = "Science", ClassId = ClassId }
        );

        // Teacher-Class-Subject (Teacher1 → Class1/Subject1)
        db.TeacherClassSubjects.Add(
            new TeacherClassSubject
            {
                Id = TeacherClassSubjectId,
                TeacherId = TeacherId,
                ClassId = ClassId,
                SubjectId = SubjectId
            }
        );

        // Enrollment: Student1 in Class1; Student2 NOT enrolled
        db.Enrollments.Add(
            new Enrollment { Id = EnrollmentId, ClassId = ClassId, StudentId = StudentId }
        );

        // Assignments (all owned by Teacher1, in Class1/Subject1)
        db.Assignments.AddRange(
            new Assignment
            {
                Id = DraftAssignmentId,
                Title = "Draft Assignment",
                Description = "A draft assignment",
                DeadlineUtc = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Draft,
                TeacherId = TeacherId,
                ClassId = ClassId,
                SubjectId = SubjectId,
                AllowResubmission = true
            },
            new Assignment
            {
                Id = PublishedAssignmentId,
                Title = "Published Assignment",
                Description = "A published assignment",
                DeadlineUtc = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                TeacherId = TeacherId,
                ClassId = ClassId,
                SubjectId = SubjectId,
                AllowResubmission = true
            },
            new Assignment
            {
                Id = PastDeadlineAssignmentId,
                Title = "Past Deadline Assignment",
                Description = "Deadline has passed",
                DeadlineUtc = DateTime.UtcNow.AddDays(-1), // past
                MaxMarks = 50,
                Status = AssignmentStatus.Published,
                TeacherId = TeacherId,
                ClassId = ClassId,
                SubjectId = SubjectId,
                AllowResubmission = true
            },
            new Assignment
            {
                Id = NoResubmissionAssignmentId,
                Title = "No Resubmission Assignment",
                Description = "Resubmission not allowed",
                DeadlineUtc = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                TeacherId = TeacherId,
                ClassId = ClassId,
                SubjectId = SubjectId,
                AllowResubmission = false
            }
        );

        // Submission (Student1 on PublishedAssignment)
        db.Submissions.Add(
            new Submission
            {
                Id = SubmissionId,
                AssignmentId = PublishedAssignmentId,
                StudentId = StudentId,
                AnswerText = "My original answer",
                SubmittedAtUtc = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            }
        );

        await db.SaveChangesAsync();
    }

    // ── Helpers ────────────────────────────────────────────────────────
    private static User NewUser(Guid id, string name, string email, UserRole role) =>
        new()
        {
            Id = id,
            Name = name,
            Email = email,
            PasswordHash = $"hash-{email}",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    // ── Service factories ──────────────────────────────────────────────
    public static AssignmentService CreateAssignmentService(AppDbContext db) => new(db);

    public static SubmissionService CreateSubmissionService(AppDbContext db) => new(db);

    public static ClassService CreateClassService(AppDbContext db) => new(db);

    public static SubjectService CreateSubjectService(AppDbContext db) => new(db);

    public static TeacherAssignmentService CreateTeacherAssignmentService(AppDbContext db) => new(db);

    public static EnrollmentService CreateEnrollmentService(AppDbContext db) => new(db);

    public static AdminReadService CreateAdminReadService(AppDbContext db) => new(db);


    public static UserService CreateUserService(AppDbContext db, IPasswordHasher? hasher = null) =>
        new(db, hasher ?? new FakePasswordHasher());

    public static AuthService CreateAuthService(
        AppDbContext db,
        IPasswordHasher? hasher = null,
        IJwtTokenService? jwt = null)
    {
        return new AuthService(
            db,
            hasher ?? new FakePasswordHasher(),
            jwt ?? new FakeJwtTokenService(),
            Options.Create(new JwtOptions
            {
                Secret = "test-secret-must-be-at-least-32-bytes-long",
                Issuer = "test-issuer",
                Audience = "test-audience",
                ExpiryMinutes = 120
            }),
            NullLogger<AuthService>.Instance
        );
    }
}
