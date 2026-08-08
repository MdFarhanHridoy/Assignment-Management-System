using AssignmentManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Data;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Class> Classes { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<TeacherClassSubject> TeacherClassSubjects { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<Submission> Submissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
