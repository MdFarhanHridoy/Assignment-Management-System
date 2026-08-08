using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Domain;
using AssignmentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Seeding;

public static class DbSeeder
{
    private static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid TeacherUserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid Teacher2UserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    private static readonly Guid StudentUserId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");

    private static readonly Guid Class9Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid Class10Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    private static readonly Guid MathId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid PhysicsId = Guid.Parse("cccccccc-0000-0000-0000-000000000002");
    private static readonly Guid EnglishId = Guid.Parse("cccccccc-0000-0000-0000-000000000003");

    private static readonly Guid TcsTeacherClass9MathId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid TcsTeacherClass9PhysicsId = Guid.Parse("dddddddd-0000-0000-0000-000000000002");
    private static readonly Guid TcsTeacherClass10EnglishId = Guid.Parse("dddddddd-0000-0000-0000-000000000003");
    private static readonly Guid TcsTeacher2Class10EnglishId = Guid.Parse("dddddddd-0000-0000-0000-000000000004");

    private static readonly Guid EnrollmentStudentClass9Id = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");

    private static readonly Guid DraftAssignmentId = Guid.Parse("ffffffff-0000-0000-0000-000000000001");
    private static readonly Guid PublishedAssignmentId = Guid.Parse("ffffffff-0000-0000-0000-000000000002");

    private static readonly Guid ReviewedSubmissionId = Guid.Parse("99999999-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher passwordHasher, CancellationToken ct = default)
    {
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        await SeedUserAsync(db, AdminUserId, "Admin User", "admin@example.com", "admin@123", UserRole.Admin, passwordHasher, now, ct);
        await SeedUserAsync(db, TeacherUserId, "Teacher One", "teacher@example.com", "teacher@123", UserRole.Teacher, passwordHasher, now, ct);
        await SeedUserAsync(db, Teacher2UserId, "Teacher Two", "teacher2@example.com", "teacher@123", UserRole.Teacher, passwordHasher, now, ct);
        await SeedUserAsync(db, StudentUserId, "Student One", "student@example.com", "student@123", UserRole.Student, passwordHasher, now, ct);

        await SeedClassAsync(db, Class9Id, "Class 9", "Ninth grade class", now, ct);
        await SeedClassAsync(db, Class10Id, "Class 10", "Tenth grade class", now, ct);

        await SeedSubjectAsync(db, MathId, "Math", Class9Id, now, ct);
        await SeedSubjectAsync(db, PhysicsId, "Physics", Class9Id, now, ct);
        await SeedSubjectAsync(db, EnglishId, "English", Class10Id, now, ct);

        await SeedTeacherClassSubjectAsync(db, TcsTeacherClass9MathId, TeacherUserId, Class9Id, MathId, now, ct);
        await SeedTeacherClassSubjectAsync(db, TcsTeacherClass9PhysicsId, TeacherUserId, Class9Id, PhysicsId, now, ct);
        await SeedTeacherClassSubjectAsync(db, TcsTeacherClass10EnglishId, TeacherUserId, Class10Id, EnglishId, now, ct);
        await SeedTeacherClassSubjectAsync(db, TcsTeacher2Class10EnglishId, Teacher2UserId, Class10Id, EnglishId, now, ct);

        await SeedEnrollmentAsync(db, EnrollmentStudentClass9Id, Class9Id, StudentUserId, now, ct);

        await SeedAssignmentAsync(
            db, DraftAssignmentId, "Draft Assignment",
            "Work in progress assignment for Class 9 Math.",
            DateTime.SpecifyKind(now.AddDays(7), DateTimeKind.Utc),
            maxMarks: 100, status: AssignmentStatus.Draft,
            teacherId: TeacherUserId, classId: Class9Id, subjectId: MathId,
            allowResubmission: true, now, ct);

        await SeedAssignmentAsync(
            db, PublishedAssignmentId, "Published Assignment",
            "Active assignment for Class 9 Math.",
            DateTime.SpecifyKind(now.AddDays(14), DateTimeKind.Utc),
            maxMarks: 100, status: AssignmentStatus.Published,
            teacherId: TeacherUserId, classId: Class9Id, subjectId: MathId,
            allowResubmission: true, now, ct);

        await SeedSubmissionAsync(
            db, ReviewedSubmissionId, PublishedAssignmentId, StudentUserId,
            "My answer to the published assignment.",
            submittedAtUtc: now, status: SubmissionStatus.Reviewed, marks: 85,
            feedback: "Good work", reviewedByTeacherId: TeacherUserId, reviewedAtUtc: now, ct);

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedUserAsync(
        AppDbContext db, Guid id, string name, string email, string password,
        UserRole role, IPasswordHasher passwordHasher, DateTime now, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Id == id, ct))
        {
            return;
        }

        db.Users.Add(new User
        {
            Id = id,
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(password),
            Role = role,
            IsActive = true,
            CreatedAt = now,
        });
    }

    private static async Task SeedClassAsync(
        AppDbContext db, Guid id, string name, string? description, DateTime now, CancellationToken ct)
    {
        if (await db.Classes.AnyAsync(c => c.Id == id, ct))
        {
            return;
        }

        db.Classes.Add(new Class
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAt = now,
        });
    }

    private static async Task SeedSubjectAsync(
        AppDbContext db, Guid id, string name, Guid classId, DateTime now, CancellationToken ct)
    {
        if (await db.Subjects.AnyAsync(s => s.Id == id, ct))
        {
            return;
        }

        db.Subjects.Add(new Subject
        {
            Id = id,
            Name = name,
            ClassId = classId,
            CreatedAt = now,
        });
    }

    private static async Task SeedTeacherClassSubjectAsync(
        AppDbContext db, Guid id, Guid teacherId, Guid classId, Guid subjectId, DateTime now, CancellationToken ct)
    {
        if (await db.TeacherClassSubjects.AnyAsync(t => t.Id == id, ct))
        {
            return;
        }

        db.TeacherClassSubjects.Add(new TeacherClassSubject
        {
            Id = id,
            TeacherId = teacherId,
            ClassId = classId,
            SubjectId = subjectId,
            CreatedAt = now,
        });
    }

    private static async Task SeedEnrollmentAsync(
        AppDbContext db, Guid id, Guid classId, Guid studentId, DateTime now, CancellationToken ct)
    {
        if (await db.Enrollments.AnyAsync(e => e.Id == id, ct))
        {
            return;
        }

        db.Enrollments.Add(new Enrollment
        {
            Id = id,
            ClassId = classId,
            StudentId = studentId,
            EnrolledAt = now,
            CreatedAt = now,
        });
    }

    private static async Task SeedAssignmentAsync(
        AppDbContext db, Guid id, string title, string description, DateTime deadlineUtc,
        int maxMarks, AssignmentStatus status, Guid teacherId, Guid classId, Guid subjectId,
        bool allowResubmission, DateTime now, CancellationToken ct)
    {
        if (await db.Assignments.AnyAsync(a => a.Id == id, ct))
        {
            return;
        }

        db.Assignments.Add(new Assignment
        {
            Id = id,
            Title = title,
            Description = description,
            DeadlineUtc = deadlineUtc,
            MaxMarks = maxMarks,
            Status = status,
            TeacherId = teacherId,
            ClassId = classId,
            SubjectId = subjectId,
            AllowResubmission = allowResubmission,
            CreatedAt = now,
        });
    }

    private static async Task SeedSubmissionAsync(
        AppDbContext db, Guid id, Guid assignmentId, Guid studentId, string answerText,
        DateTime submittedAtUtc, SubmissionStatus status, int? marks, string? feedback,
        Guid? reviewedByTeacherId, DateTime? reviewedAtUtc, CancellationToken ct)
    {
        if (await db.Submissions.AnyAsync(s => s.Id == id, ct))
        {
            return;
        }

        db.Submissions.Add(new Submission
        {
            Id = id,
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = answerText,
            SubmittedAtUtc = submittedAtUtc,
            Status = status,
            Marks = marks,
            Feedback = feedback,
            ReviewedByTeacherId = reviewedByTeacherId,
            ReviewedAtUtc = reviewedAtUtc,
        });
    }
}
